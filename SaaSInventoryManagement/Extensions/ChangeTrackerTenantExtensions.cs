using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
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

                if (entry.State == EntityState.Added)
                {
                    if (tenantProvider.IsSuperAdmin)
                        continue;

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