using Microsoft.EntityFrameworkCore.Storage;
using SaaSInventoryManagement.Models;
using SaaSInventoryManagement.Models.Base;
using SaaSInventoryManagement.Repositories.Interfaces;

namespace SaaSInventoryManagement.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _db;
        private readonly Dictionary<Type, object> _genericRepositories = new();

        public UnitOfWork(
            ApplicationDbContext db,
            ICategoryRepository categories,
            IWarehouseRepository warehouses,
            IProductRepository products,
            ISupplierRepository suppliers,
            ICustomerRepository customers,
            IPurchaseOrderRepository purchaseOrders)
        {
            _db = db;
            Categories = categories;
            Warehouses = warehouses;
            Products = products;
            Suppliers =  suppliers;
            Customers = customers;
            PurchaseOrders = purchaseOrders;
        }

        public ICategoryRepository Categories { get; }
        public IWarehouseRepository Warehouses { get; }
        public IProductRepository Products { get; }
        public ISupplierRepository Suppliers { get; }
        public ICustomerRepository Customers { get; }
        public IPurchaseOrderRepository PurchaseOrders { get; }
        public IGenericRepository<TEntity> Repository<TEntity>() where TEntity : class, IEntity
        {
            if (!_genericRepositories.TryGetValue(typeof(TEntity), out var repository))
            {
                repository = new GenericRepository<TEntity>(_db);
                _genericRepositories[typeof(TEntity)] = repository;
            }

            return (IGenericRepository<TEntity>)repository;
        }

        public Task<int> SaveChangesAsync() => _db.SaveChangesAsync();
        public Task<IDbContextTransaction> BeginTransactionAsync() => _db.Database.BeginTransactionAsync();
    }
}
