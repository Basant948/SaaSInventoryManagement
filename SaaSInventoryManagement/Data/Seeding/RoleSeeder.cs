using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace SaaSInventoryManagement.Data.Seeding
{
    public static class RoleSeeder
    {
        private const string SeedKey = "roles:v1";

        public static readonly string[] Roles =
        {
            "SuperAdmin",
            "TenantAdmin",
            "User",
        };

        public static async Task SeedAsync(ApplicationDbContext db, RoleManager<IdentityRole> roleManager)
        {
            if (await db.SeedHistory.AnyAsync(s => s.SeedKey == SeedKey))
                return;

            foreach (var role in Roles)
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));

            db.SeedHistory.Add(new Models.SeedHistory
            {
                SeedKey = SeedKey,
                AppliedAt = DateTime.UtcNow,
                Notes = $"Seeded roles: {string.Join(", ", Roles)}"
            });
            await db.SaveChangesAsync();
        }
    }
}
