using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using SaaSInventoryManagement.Configurations;
using SaaSInventoryManagement.Extensions;
using SaaSInventoryManagement.Models;
using SaaSInventoryManagement.Models.Identity;
using SaaSInventoryManagement.Services.Interfaces_;

public class ApplicationDbContext : IdentityDbContext<Applicationuser>
{
    private readonly ITenantProvider _tenantProvider;

    //internal ITenantProvider TenantProvider => _tenantProvider;
    // this above OR this downn works same.needed for the extension methods to access the tenant provider from the context.
    internal ITenantProvider TenantProvider
    {
        get { return _tenantProvider; }
    }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> dbContext, ITenantProvider tenantProvider) : base(dbContext)
    {
        _tenantProvider = tenantProvider;
    }

    public DbSet<Tenant> Tenants { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }
    public DbSet<Permission> Permissions { get; set; }
    public DbSet<UserPermission> UserPermissions { get; set; }
    public DbSet<SeedHistory> SeedHistory { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ApplyConfiguration(new AuditLogConfiguration());

        builder.ApplyTenantQueryFilters(this);   
        builder.EnsureNoUnprotectedTenantEntities();
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        ChangeTracker.ApplyTenantWriteGuards(_tenantProvider);
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        ChangeTracker.ApplyTenantWriteGuards(_tenantProvider);
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }
}