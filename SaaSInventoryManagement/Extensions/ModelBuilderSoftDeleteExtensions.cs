using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using SaaSInventoryManagement.Models.Base;
using System.Linq.Expressions;
using System.Reflection;

namespace SaaSInventoryManagement.Extensions
{
    public static class ModelBuilderSoftDeleteExtensions
    {
        private static readonly MethodInfo SetSoftDeleteQueryFilterMethod =
            typeof(ModelBuilderSoftDeleteExtensions).GetMethod(
                nameof(SetSoftDeleteQueryFilter),
                BindingFlags.NonPublic | BindingFlags.Static)!;

        public static void ApplySoftDeleteQueryFilters(this ModelBuilder builder)
        {
            foreach (var entityType in builder.Model.GetEntityTypes())
            {
                if (!typeof(ISoftDelete).IsAssignableFrom(entityType.ClrType))
                    continue;

                SetSoftDeleteQueryFilterMethod
                    .MakeGenericMethod(entityType.ClrType)
                    .Invoke(null, new object[] { builder });
            }
        }

        private static void SetSoftDeleteQueryFilter<TEntity>(ModelBuilder builder)
            where TEntity : class, ISoftDelete
        {
            var entity = builder.Entity<TEntity>();

            Expression<Func<TEntity, bool>> notDeleted = e => !e.IsDeleted;

            var existingFilter = entity.Metadata.GetQueryFilter();
            if (existingFilter is null)
            {
                entity.HasQueryFilter(notDeleted);
                return;
            }

            var parameter = Expression.Parameter(typeof(TEntity), "e");

            var existingBody = ReplacingExpressionVisitor.Replace(
                existingFilter.Parameters[0], parameter, existingFilter.Body);

            var newBody = ReplacingExpressionVisitor.Replace(
                notDeleted.Parameters[0], parameter, notDeleted.Body);

            var combined = Expression.Lambda<Func<TEntity, bool>>(
                Expression.AndAlso(existingBody, newBody), parameter);

            entity.HasQueryFilter(combined);
        }
    }
}