using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SaaSInventoryManagement.Data.Seeding;
using SaaSInventoryManagement.Models.Identity;

namespace SaaSInventoryManagement.Data
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(IServiceProvider services)
        {
            var db = services.GetRequiredService<ApplicationDbContext>();
            var userManager = services.GetRequiredService<UserManager<Applicationuser>>();
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            var logger = services.GetRequiredService<ILogger<ApplicationDbContext>>();

            //   Apply pending migrations 
            try
            {
                await db.Database.MigrateAsync();
            }
            catch (Exception ex)
            {
                logger.LogCritical(ex,
                    "Migration failed. Drop the database and re-run migrations to rebuild from scratch.");
                throw;
            }

            //  Seed data (order matters)
            await RoleSeeder.SeedAsync(db, roleManager);
            await PermissionSeeder.SeedAsync(db);
            await TenantSeeder.SeedAsync(db);
            await SuperAdminSeeder.SeedAsync(db, userManager);
        }
    }
}
