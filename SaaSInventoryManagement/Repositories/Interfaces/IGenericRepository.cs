using SaaSInventoryManagement.Models.Base;
using System.Linq.Expressions;

namespace SaaSInventoryManagement.Repositories.Interfaces
{
    public interface IGenericRepository<TEntity> where TEntity : class, IEntity
    {
        Task<TEntity?> GetByIdAsync(int id);
        Task<List<TEntity>> GetAllAsync();
        Task<List<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate);
        Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate);
        Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate);

  
        IQueryable<TEntity> Query();

        Task AddAsync(TEntity entity);
        void Update(TEntity entity);
        void Remove(TEntity entity);
    }
}
