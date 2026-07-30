using SaaSInventoryManagement.Models.Identity;

namespace SaaSInventoryManagement.Models
{
    public class UserPermission
    {
        public int Id { get; set; }

        public string UserId { get; set; }
        public Applicationuser User { get; set; }

        public int PermissionId { get; set; }
        public Permission Permission { get; set; }

        public DateTime GrantedAt { get; set; } = DateTime.UtcNow;
    }
}
