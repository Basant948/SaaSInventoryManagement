using Microsoft.AspNetCore.Identity;

namespace SaaSInventoryManagement.Models.Identity
{
    public class Applicationuser : IdentityUser
    {
        public int FirstName { get; set; }
        public int LastName { get; set; }
    }
}
