using Microsoft.EntityFrameworkCore;
using SaaSInventoryManagement.Models;
using SaaSInventoryManagement.Repositories.Interfaces;

namespace SaaSInventoryManagement.Repositories
{
    public class SalesOrderRepository : GenericRepository<SalesOrder>, ISalesOrderRepository
    {
        public SalesOrderRepository(ApplicationDbContext db) : base(db)
        {
        }

        public async Task<SalesOrder?> GetWithLinesAsync(int id)
        {
            return await DbSet
                .Include(so => so.Customer)
                .Include(so => so.Warehouse)
                .Include(so => so.Lines).ThenInclude(l => l.Product)
                .FirstOrDefaultAsync(so => so.Id == id);
        }

        public async Task<bool> SoNumberExistsAsync(string soNumber)
        {
            var normalized = soNumber.Trim().ToLower();
            return await DbSet.AnyAsync(so => so.SoNumber.ToLower() == normalized);
        }
    }
}
