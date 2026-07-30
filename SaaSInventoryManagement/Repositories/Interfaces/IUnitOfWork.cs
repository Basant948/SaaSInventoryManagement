using SaaSInventoryManagement.Models.Base;

namespace SaaSInventoryManagement.Repositories.Interfaces
{
    public interface IUnitOfWork
    {

        IGenericRepository<TEntity> Repository<TEntity>() where TEntity : class, IEntity;

        Task<int> SaveChangesAsync();
    }
}
