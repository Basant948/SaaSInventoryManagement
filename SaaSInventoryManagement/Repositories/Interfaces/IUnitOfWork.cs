using SaaSInventoryManagement.Models.Base;

namespace SaaSInventoryManagement.Repositories.Interfaces
{
    public interface IUnitOfWork
    {
        ICategoryRepository Categories { get; }
        IWarehouseRepository Warehouses { get; }
        IProductRepository Products { get; }
        IGenericRepository<TEntity> Repository<TEntity>() where TEntity : class, IEntity;

        Task<int> SaveChangesAsync();
    }
}
