using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using SaaSInventoryManagement.Models.Identity;
using System.Security.Claims;

namespace SaaSInventoryManagement.Services
{
    public class TenantClaimsPrincipalFactory : UserClaimsPrincipalFactory<Applicationuser, IdentityRole>
    {
        public TenantClaimsPrincipalFactory(
            UserManager<Applicationuser> userManager,
            RoleManager<IdentityRole> roleManager,
            IOptions<IdentityOptions> optionsAccessor)
            : base(userManager, roleManager, optionsAccessor)
        {
        }

        public override async Task<ClaimsPrincipal> CreateAsync(Applicationuser user)
        {
            var principal = await base.CreateAsync(user);

            if (user.TenantId.HasValue && principal.Identity is ClaimsIdentity identity)
            {
                identity.AddClaim(new Claim(TenantProvider.TenantIdClaimType, user.TenantId.Value.ToString()));
            }

            return principal;
        }
    }
}

