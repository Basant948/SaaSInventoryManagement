using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using SaaSInventoryManagement.Models.Base;

namespace SaaSInventoryManagement.Extensions
{
    public static class ChangeTrackerSoftDeleteExtensions
    {
        public static void ApplySoftDeleteInterception(this ChangeTracker changeTracker, string? currentUserId)
        {
            var entries = changeTracker.Entries()
                .Where(e => e.Entity is ISoftDelete && e.State == EntityState.Deleted)
                .ToList();

            foreach (var entry in entries)
            {
                var softDelete = (ISoftDelete)entry.Entity;

                entry.State = EntityState.Modified;

                softDelete.IsDeleted = true;
                softDelete.DeletedAt = DateTime.UtcNow;
                softDelete.DeletedBy = currentUserId;

                foreach (var property in entry.Properties)
                {
                    var name = property.Metadata.Name;
                    property.IsModified =
                        name == nameof(ISoftDelete.IsDeleted) ||
                        name == nameof(ISoftDelete.DeletedAt) ||
                        name == nameof(ISoftDelete.DeletedBy);
                }
            }
        }
    }
}
