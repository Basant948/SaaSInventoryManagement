using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using SaaSInventoryManagement.Infrastructure.Authorization;
using SaaSInventoryManagement.Models.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace SaaSInventoryManagement.Services
{
    public class TenantClaimsPrincipalFactory : UserClaimsPrincipalFactory<Applicationuser, IdentityRole>
    {
        private readonly ApplicationDbContext _db;

        public TenantClaimsPrincipalFactory(
            UserManager<Applicationuser> userManager,
            RoleManager<IdentityRole> roleManager,
            IOptions<IdentityOptions> optionsAccessor,
            ApplicationDbContext db)
            : base(userManager, roleManager, optionsAccessor)
        {
            _db = db;
        }

        protected override async Task<ClaimsIdentity> GenerateClaimsAsync(Applicationuser user)
        {
            // Let Identity create its default claims first
            var identity = await base.GenerateClaimsAsync(user);

            // Add tenant claim so we know which tenant the user belongs to
            if (user.TenantId.HasValue)
                identity.AddClaim(new Claim(TenantProvider.TenantIdClaimType, user.TenantId.Value.ToString()));

            var roles = await UserManager.GetRolesAsync(user);

            if (roles.Contains("SuperAdmin") || roles.Contains("TenantAdmin"))
            {
                // Admins can access everything
                identity.AddClaim(new Claim(PermissionClaimTypes.Permission, "*"));
            }
            else
            {
                // Load only the permissions assigned to this user
                var permissionKeys = await _db.UserPermissions
                    .Where(up => up.UserId == user.Id && up.Permission.IsActive)
                    .Select(up => up.Permission.Key)
                    .ToListAsync();

                // Add each permission as a claim
                foreach (var key in permissionKeys)
                    identity.AddClaim(new Claim(PermissionClaimTypes.Permission, key));
            }

            return identity;
        }
    }
}

