using Microsoft.AspNetCore.Identity;
using SaaSInventoryManagement.Models;
using SaaSInventoryManagement.Models.Identity;
using Microsoft.EntityFrameworkCore;

namespace SaaSInventoryManagement.Data.Seeding
{
    public static class SuperAdminSeeder
    {
        private const string SeedKey = "superadmin:v1";

        public const string SuperAdminEmail = "superadmin@inventorywithai.com";
        public const string SuperAdminPassword = "SuperAdmin@123";

        public const string DemoAdminEmail = "admin@demoinventory.com";
        public const string DemoAdminPassword = "Admin@123";

        public const string DemoStaffEmail = "staff@demoinventory.com";
        public const string DemoStaffPassword = "Staff@123";

        // Keys the demo staff user can see 
        private static readonly string[] DemoStaffPermissionKeys =
        {
            "inv.dashboard.view",
            "inv.products.view",
            "inv.stock.view",
        };

        public static async Task SeedAsync(ApplicationDbContext db, UserManager<Applicationuser> userManager)
        {
            if (await db.SeedHistory.AnyAsync(s => s.SeedKey == SeedKey))
                return;

            // SuperAdmin
            if (await userManager.FindByEmailAsync(SuperAdminEmail) == null)
            {
                var superAdmin = new Applicationuser
                {
                    FirstName = "Super",
                    LastName = "Admin",
                    UserName = SuperAdminEmail,
                    Email = SuperAdminEmail,
                    TenantId = null,
                    EmailConfirmed = true
                };
                var result = await userManager.CreateAsync(superAdmin, SuperAdminPassword);
                if (result.Succeeded)
                    await userManager.AddToRoleAsync(superAdmin, "SuperAdmin");
            }

            var demoTenant = await db.Tenants.FirstOrDefaultAsync(t => t.Slug == TenantSeeder.DemoTenantSlug);

            //  TenantAdmin for the demo tenant 
            if (demoTenant != null && await userManager.FindByEmailAsync(DemoAdminEmail) == null)
            {
                var tenantAdmin = new Applicationuser
                {
                    FirstName = "Demo",
                    LastName = "Admin",
                    UserName = DemoAdminEmail,
                    Email = DemoAdminEmail,
                    TenantId = demoTenant.Id,
                    EmailConfirmed = true
                };
                var result = await userManager.CreateAsync(tenantAdmin, DemoAdminPassword);
                if (result.Succeeded)
                    await userManager.AddToRoleAsync(tenantAdmin, "TenantAdmin");
            }

            //  Demo staff 
            if (demoTenant != null && await userManager.FindByEmailAsync(DemoStaffEmail) == null)
            {
                var staff = new Applicationuser
                {
                    FirstName = "Demo",
                    LastName = "Staff",
                    UserName = DemoStaffEmail,
                    Email = DemoStaffEmail,
                    TenantId = demoTenant.Id,
                    EmailConfirmed = true
                };
                var result = await userManager.CreateAsync(staff, DemoStaffPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(staff, "User");

                    var permissionIds = await db.Permissions
                        .Where(p => DemoStaffPermissionKeys.Contains(p.Key))
                        .Select(p => p.Id)
                        .ToListAsync();

                    foreach (var permissionId in permissionIds)
                    {
                        db.UserPermissions.Add(new UserPermission
                        {
                            UserId = staff.Id,
                            PermissionId = permissionId,
                            GrantedAt = DateTime.UtcNow
                        });
                    }
                }
            }

            db.SeedHistory.Add(new SeedHistory
            {
                SeedKey = SeedKey,
                AppliedAt = DateTime.UtcNow,
                Notes = "Seeded SuperAdmin, demo TenantAdmin, and demo staff User"
            });
            await db.SaveChangesAsync();
        }
    }
}
