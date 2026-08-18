using Microsoft.EntityFrameworkCore.Storage;
using SaaSInventoryManagement.Models.Base;

namespace SaaSInventoryManagement.Repositories.Interfaces
{
    public interface IUnitOfWork
    {
        ICategoryRepository Categories { get; }
        IWarehouseRepository Warehouses { get; }
        IProductRepository Products { get; }
        ISupplierRepository Suppliers { get; }
        ICustomerRepository Customers { get; }
        IGenericRepository<TEntity> Repository<TEntity>() where TEntity : class, IEntity;

        Task<int> SaveChangesAsync();
        Task<IDbContextTransaction> BeginTransactionAsync();
    }
}
