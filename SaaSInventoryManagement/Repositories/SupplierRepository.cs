using SaaSInventoryManagement.Models;
using SaaSInventoryManagement.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace SaaSInventoryManagement.Repositories
{
    public class SupplierRepository : GenericRepository<Supplier>, ISupplierRepository
    {
        public SupplierRepository(ApplicationDbContext db) : base(db)
        {
        }

        public async Task<bool> NameExistsAsync(string name, int? excludingId = null)
        {
            var normalized = name.Trim().ToLower();

            return await DbSet.AnyAsync(s =>
                s.Name.ToLower() == normalized &&
                (excludingId == null || s.Id != excludingId));
        }
    }
}
