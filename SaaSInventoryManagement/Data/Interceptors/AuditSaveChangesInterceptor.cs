using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using SaaSInventoryManagement.Enums;
using SaaSInventoryManagement.Models;
using SaaSInventoryManagement.Models.Base;
using SaaSInventoryManagement.Services.Interfaces_;
using System.Text.Json;

namespace SaaSInventoryManagement.Data.Interceptors
{
    public class AuditSaveChangesInterceptor : SaveChangesInterceptor
    {
        private readonly ICurrentUserService _currentUser;
        private readonly ITenantProvider _tenantProvider;

        public AuditSaveChangesInterceptor(ICurrentUserService currentUser, ITenantProvider tenantProvider)
        {
            _currentUser = currentUser;
            _tenantProvider = tenantProvider;
        }

        public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
        {
            AddAuditLogs(eventData.Context);
            return base.SavingChanges(eventData, result);
        }

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            AddAuditLogs(eventData.Context);
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        private void AddAuditLogs(DbContext context)
        {
            if (context is null) return;

            var entries = context.ChangeTracker.Entries()
                .Where(e => e.Entity is ITenantOwned
                    && e.Entity is not AuditLog
                    && e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
                .ToList();

            foreach (var entry in entries)
            {
                context.Set<AuditLog>().Add(BuildAuditLog(entry));
            }
        }

        private AuditLog BuildAuditLog(EntityEntry entry)
        {
            var tenantOwned = (ITenantOwned)entry.Entity;

            return new AuditLog
            {
                // Fallback to the ambient tenant if the entity's own TenantId
                // isn't reliable yet for a brand-new row - see note in the
                // write-guard about ordering, but for Added rows the guard
                // already ran and set this correctly, so this is mostly a
                // safety net for SuperAdmin-authored writes.
                TenantId = tenantOwned.TenantId != 0 ? tenantOwned.TenantId : (_tenantProvider.TenantId ?? 0),
                UserId = _currentUser.UserId,
                Username = _currentUser.Username,
                Action = entry.State switch
                {
                    EntityState.Added => AuditAction.Create,
                    EntityState.Modified => AuditAction.Update,
                    EntityState.Deleted => AuditAction.Delete,
                    _ => throw new InvalidOperationException($"Unexpected entity state '{entry.State}' for auditing.")
                },
                EntityName = entry.Entity.GetType().Name,
                EntityId = string.Join(",", entry.Properties
                    .Where(p => p.Metadata.IsPrimaryKey())
                    .Select(p => p.CurrentValue?.ToString() ?? "null")),
                OldValues = SerializeOldValues(entry),
                NewValues = SerializeNewValues(entry),
                CorrelationId = _currentUser.CorrelationId,
                IPAddress = _currentUser.IPAddress,
                RequestPath = _currentUser.RequestPath,
                CreatedAt = DateTime.UtcNow
            };
        }

        private static string SerializeOldValues(EntityEntry entry)
        {
            if (entry.State == EntityState.Added) return null;
            var props = entry.State == EntityState.Modified
                ? entry.Properties.Where(p => p.IsModified)
                : entry.Properties;
            return Serialize(props.ToDictionary(p => p.Metadata.Name, p => p.OriginalValue));
        }

        private static string SerializeNewValues(EntityEntry entry)
        {
            if (entry.State == EntityState.Deleted) return null;
            var props = entry.State == EntityState.Modified
                ? entry.Properties.Where(p => p.IsModified)
                : entry.Properties;
            return Serialize(props.ToDictionary(p => p.Metadata.Name, p => p.CurrentValue));
        }

        private static string Serialize(Dictionary<string, object> values) =>
            values.Count == 0 ? null : JsonSerializer.Serialize(values);
    }
}