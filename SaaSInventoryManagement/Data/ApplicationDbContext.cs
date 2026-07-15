using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SaaSInventoryManagement.Models.Identity;

namespace SaaSInventoryManagement.Data
{
    public class ApplicationDbContext : IdentityDbContext<Applicationuser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> dbContext) :base(dbContext)
        {
            
        }
    }
}
