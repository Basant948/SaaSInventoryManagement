using SaaSInventoryManagement.Models;
using SaaSInventoryManagement.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace SaaSInventoryManagement.Repositories
{
    public class CategoryRepository : GenericRepository<Category>, ICategoryRepository
    {
        public CategoryRepository(ApplicationDbContext db) : base(db)
        {
        }

        public async Task<bool> NameExistsAsync(string name, int? excludingId = null)
        {
            var normalized = name.Trim().ToLower();

            return await DbSet.AnyAsync(c =>
                c.Name.ToLower() == normalized &&
                (excludingId == null || c.Id != excludingId));
        }
    }
}
