using SaaSInventoryManagement.Models;
using SaaSInventoryManagement.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace SaaSInventoryManagement.Repositories
{
    public class WarehouseRepository : GenericRepository<Warehouse>, IWarehouseRepository
    {
        public WarehouseRepository(ApplicationDbContext db) : base(db)
        {
        }

        public async Task<bool> CodeExistsAsync(string code, int? excludingId = null)
        {
            var normalized = code.Trim().ToLower();

            return await DbSet.AnyAsync(w =>
                w.Code.ToLower() == normalized &&
                (excludingId == null || w.Id != excludingId));
        }
    }
}
