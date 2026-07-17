using Microsoft.EntityFrameworkCore;
using SaaSInventoryManagement.Models.Base;
using SaaSInventoryManagement.Models.Identity;
using SaaSInventoryManagement.Services.Interfaces_;
using System.Reflection;

namespace SaaSInventoryManagement.Extensions
{
    public static class ModelBuilderTenantExtensions
    {
        private static readonly MethodInfo SetTenantQueryFilterMethod =
            typeof(ModelBuilderTenantExtensions).GetMethod(
                nameof(SetTenantQueryFilter),
                BindingFlags.NonPublic | BindingFlags.Static)!;

        public static void ApplyTenantQueryFilters(this ModelBuilder builder, ITenantProvider tenantProvider)
        {
            foreach (var entityType in builder.Model.GetEntityTypes())
            {
                if (!typeof(ITenantOwned).IsAssignableFrom(entityType.ClrType))
                    continue;

                SetTenantQueryFilterMethod
                    .MakeGenericMethod(entityType.ClrType)
                    .Invoke(null, new object[] { builder, tenantProvider });
            }
        }

        private static void SetTenantQueryFilter<TEntity>(ModelBuilder builder, ITenantProvider tenantProvider)
            where TEntity : class, ITenantOwned
        {
            builder.Entity<TEntity>()
                .HasQueryFilter(e => tenantProvider.IsSuperAdmin || e.TenantId == tenantProvider.TenantId);
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
                        $"ITenantOwned, so it will NOT be tenant-isolated - any signed-in user could " +
                        $"read every tenant's rows for it. Fix: add 'ITenantOwned' to the class " +
                        $"declaration for '{clrType.Name}'.");
                }
            }
        }
    }
}