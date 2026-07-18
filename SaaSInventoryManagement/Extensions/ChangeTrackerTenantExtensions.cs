using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using SaaSInventoryManagement.Models;
using SaaSInventoryManagement.Models.Base;
using SaaSInventoryManagement.Services.Interfaces_;

namespace SaaSInventoryManagement.Extensions
{
    public static class ChangeTrackerTenantExtensions
    {
        public static void ApplyTenantWriteGuards(this ChangeTracker changeTracker, ITenantProvider tenantProvider)
        {
            foreach (var entry in changeTracker.Entries())
            {
                if (entry.Entity is not ITenantOwned tenantOwned)
                    continue;
                if (entry.Entity is AuditLog && entry.State != EntityState.Added)
                {
                    throw new InvalidOperationException(
                        "AuditLog rows are append-only and cannot be modified or deleted.");
                }

                if (entry.State == EntityState.Added)
                {
                    if (tenantProvider.IsSuperAdmin)
                    {
                        if (tenantOwned.TenantId == 0)
                            throw new InvalidOperationException(
                                $"SuperAdmin must explicitly set TenantId when creating '{entry.Entity.GetType().Name}'.");
                        continue;
                    }

                    if (tenantProvider.TenantId is null)
                    {

                        throw new InvalidOperationException(
                            $"Cannot save a new '{entry.Entity.GetType().Name}' - no tenant is resolved " +
                            "for the current user.");
                    }


                    tenantOwned.TenantId = tenantProvider.TenantId.Value;
                }
                else if (entry.State == EntityState.Modified)
                {
                    var tenantIdProperty = entry.Property(nameof(ITenantOwned.TenantId));

                    if (tenantIdProperty.IsModified && !tenantProvider.IsSuperAdmin)
                    {

                        var keyValues = entry.Properties
                            .Where(p => p.Metadata.IsPrimaryKey())
                            .Select(p => p.CurrentValue?.ToString() ?? "null");

                        throw new InvalidOperationException(
                            $"Changing the TenantId of an existing '{entry.Entity.GetType().Name}' " +
                            $"(key: {string.Join(",", keyValues)}) is not allowed. A tenant-owned row " +
                            "must never move to a different tenant after it's created.");
                    }
                }
            }
        }
    }
}