using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SaaSInventoryManagement.Data.Extensions;
using SaaSInventoryManagement.Models;
using SaaSInventoryManagement.Models.Identity;
using SaaSInventoryManagement.Services.Interfaces_;

namespace SaaSInventoryManagement.Data
{
    public class ApplicationDbContext : IdentityDbContext<Applicationuser>
    {
        private readonly ITenantProvider _tenantProvider;
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> dbContext, ITenantProvider tenantProvider) : base(dbContext)
        {
            _tenantProvider = tenantProvider;
        }
        public DbSet<Tenant> Tenants { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.ApplyTenantQueryFilters(_tenantProvider);
        }
    }
}
