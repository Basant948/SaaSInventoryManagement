using Microsoft.EntityFrameworkCore;
using SaaSInventoryManagement.Models;
using SaaSInventoryManagement.Repositories.Interfaces;

namespace SaaSInventoryManagement.Repositories
{
    public class PurchaseOrderRepository : GenericRepository<PurchaseOrder>, IPurchaseOrderRepository
    {
        public PurchaseOrderRepository(ApplicationDbContext db) : base(db)
        {
        }

        public async Task<PurchaseOrder?> GetWithLinesAsync(int id)
        {
            return await DbSet
                .Include(po => po.Supplier)
                .Include(po => po.Warehouse)
                .Include(po => po.Lines).ThenInclude(l => l.Product)
                .FirstOrDefaultAsync(po => po.Id == id);
        }

        public async Task<bool> PoNumberExistsAsync(string poNumber)
        {
            var normalized = poNumber.Trim().ToLower();
            return await DbSet.AnyAsync(po => po.PoNumber.ToLower() == normalized);
        }
    }
}
