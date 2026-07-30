using Microsoft.EntityFrameworkCore;
using SaaSInventoryManagement.Data;
using SaaSInventoryManagement.Models.Base;
using SaaSInventoryManagement.Models.Identity;
using System.Reflection;

namespace SaaSInventoryManagement.Extensions
{
    public static class ModelBuilderTenantExtensions
    {
        private static readonly MethodInfo SetTenantQueryFilterMethod =
            typeof(ModelBuilderTenantExtensions).GetMethod(
                nameof(SetTenantQueryFilter),
                BindingFlags.NonPublic | BindingFlags.Static)!;

        public static void ApplyTenantQueryFilters(this ModelBuilder builder, ApplicationDbContext context)
        {
            foreach (var entityType in builder.Model.GetEntityTypes())
            {
                if (!typeof(ITenantOwned).IsAssignableFrom(entityType.ClrType))
                    continue;

                SetTenantQueryFilterMethod
                    .MakeGenericMethod(entityType.ClrType)
                    .Invoke(null, new object[] { builder, context });
            }
        }

        private static void SetTenantQueryFilter<TEntity>(ModelBuilder builder, ApplicationDbContext context)
            where TEntity : class, ITenantOwned
        {
            builder.Entity<TEntity>()
                .HasQueryFilter(e => context.TenantProvider.IsSuperAdmin || e.TenantId == context.TenantProvider.TenantId);
        }

        public static void EnsureNoUnprotectedTenantEntities(this ModelBuilder builder)
        {
            foreach (var entityType in builder.Model.GetEntityTypes())
            {
                var clrType = entityType.ClrType;

                if (typeof(ITenantOwned).IsAssignableFrom(clrType))
                    continue;

                if (typeof(Applicationuser).IsAssignableFrom(clrType) ||
                    clrType.Namespace?.StartsWith("Microsoft.AspNetCore.Identity") == true)
                    continue;

                var hasTenantIdProperty =
                    clrType.GetProperty("TenantId") != null ||
                    entityType.FindProperty("TenantId") != null;

                if (hasTenantIdProperty)
                {
                    throw new InvalidOperationException(
                        $"Entity '{clrType.Name}' has a TenantId property but does not implement " +
                        $"ITenantOwned, so it will NOT be tenant-isolated. Fix: add 'ITenantOwned' to '{clrType.Name}'.");
                }
            }
        }
    }
}