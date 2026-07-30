using SaaSInventoryManagement.Models.Base;
using SaaSInventoryManagement.Repositories.Interfaces;

namespace SaaSInventoryManagement.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _db;
        private readonly Dictionary<Type, object> _genericRepositories = new();

        public UnitOfWork(ApplicationDbContext db)
        {
            _db = db;
        }

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
    }
}
