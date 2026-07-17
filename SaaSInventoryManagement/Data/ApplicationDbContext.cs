using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SaaSInventoryManagement.Models;
using SaaSInventoryManagement.Models.Identity;

namespace SaaSInventoryManagement.Data
{
    public class ApplicationDbContext : IdentityDbContext<Applicationuser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> dbContext) :base(dbContext)
        {
            
        }
        public DbSet<Tenant> Tenants { get; set; }
    }
}
