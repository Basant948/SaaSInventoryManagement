using Microsoft.AspNetCore.Identity;

namespace SaaSInventoryManagement.Models.Identity
{
    public class Applicationuser : IdentityUser
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
    }
}
