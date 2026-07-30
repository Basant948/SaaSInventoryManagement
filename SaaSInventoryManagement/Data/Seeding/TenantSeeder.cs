using SaaSInventoryManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace SaaSInventoryManagement.Data.Seeding
{
    public static class TenantSeeder
    {
        private const string SeedKey = "tenants:v1";

        public const string DemoTenantSlug = "demo-inventory";

        public static async Task SeedAsync(ApplicationDbContext db)
        {
            if (await db.SeedHistory.AnyAsync(s => s.SeedKey == SeedKey))
                return;

            if (!await db.Tenants.AnyAsync(t => t.Slug == DemoTenantSlug))
            {
                db.Tenants.Add(new Tenant
                {
                    Name = "Demo Inventory Co.",
                    Slug = DemoTenantSlug,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                });
                await db.SaveChangesAsync();
            }

            db.SeedHistory.Add(new SeedHistory
            {
                SeedKey = SeedKey,
                AppliedAt = DateTime.UtcNow,
                Notes = "Seeded demo tenant"
            });
            await db.SaveChangesAsync();
        }
    }
}
