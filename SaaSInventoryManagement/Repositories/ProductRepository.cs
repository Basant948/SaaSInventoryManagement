using SaaSInventoryManagement.Models;
using SaaSInventoryManagement.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace SaaSInventoryManagement.Repositories
{
    public class ProductRepository : GenericRepository<Product>, IProductRepository
    {
        public ProductRepository(ApplicationDbContext db) : base(db)
        {
        }

        public async Task<bool> SkuExistsAsync(string sku, int? excludingId = null)
        {
            var normalized = sku.Trim().ToLower();

            return await DbSet.AnyAsync(p =>
                p.SKU.ToLower() == normalized &&
                (excludingId == null || p.Id != excludingId));
        }
    }
}
