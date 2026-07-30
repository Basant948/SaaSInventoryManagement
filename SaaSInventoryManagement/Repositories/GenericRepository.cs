using Microsoft.EntityFrameworkCore;
using SaaSInventoryManagement.Models.Base;
using SaaSInventoryManagement.Repositories.Interfaces;
using System.Linq.Expressions;

namespace SaaSInventoryManagement.Repositories
{
    public class GenericRepository<TEntity> : IGenericRepository<TEntity> where TEntity : class, IEntity
    {
        protected readonly ApplicationDbContext Db;
        protected readonly DbSet<TEntity> DbSet;

        public GenericRepository(ApplicationDbContext db)
        {
            Db = db;
            DbSet = db.Set<TEntity>();
        }

        public async Task<TEntity?> GetByIdAsync(int id) =>
            await DbSet.FirstOrDefaultAsync(e => e.Id == id);

        public async Task<List<TEntity>> GetAllAsync() =>
            await DbSet.AsNoTracking().ToListAsync();

        public async Task<List<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate) =>
            await DbSet.AsNoTracking().Where(predicate).ToListAsync();

        public async Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate) =>
            await DbSet.FirstOrDefaultAsync(predicate);

        public async Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate) =>
            await DbSet.AnyAsync(predicate);

        public IQueryable<TEntity> Query() => DbSet.AsQueryable();

        public async Task AddAsync(TEntity entity) => await DbSet.AddAsync(entity);

        public void Update(TEntity entity)
        {

            if (Db.Entry(entity).State == EntityState.Detached)
                DbSet.Update(entity);
        }

        public void Remove(TEntity entity) => DbSet.Remove(entity);
    }
}
