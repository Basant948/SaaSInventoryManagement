using Microsoft.AspNetCore.Authorization;

namespace SaaSInventoryManagement.Infrastructure.Authorization
{
    public sealed class PermissionRequirement : IAuthorizationRequirement
    {
        public string Key { get; }

        public PermissionRequirement(string key) => Key = key;
    }
}
