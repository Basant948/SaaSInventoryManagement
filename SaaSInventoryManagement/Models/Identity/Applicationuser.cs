using Microsoft.AspNetCore.Identity;

namespace SaaSInventoryManagement.Models.Identity
{
    public class Applicationuser : IdentityUser
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;

        public int? TenantId { get; set; }
        public Tenant Tenant { get; set; }

        public bool IsPermissionManaged { get; set; } = false;
    }
}
