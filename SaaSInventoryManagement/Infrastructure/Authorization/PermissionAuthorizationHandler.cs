using Microsoft.AspNetCore.Authorization;

namespace SaaSInventoryManagement.Infrastructure.Authorization
{
    public sealed class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
    {
        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            PermissionRequirement requirement)
        {
            var claims = context.User.FindAll(PermissionClaimTypes.Permission);

            foreach (var claim in claims)
            {
                if (claim.Value == "*" || claim.Value == requirement.Key)
                {
                    context.Succeed(requirement);
                    break;
                }
            }

            return Task.CompletedTask;
        }
    }
}