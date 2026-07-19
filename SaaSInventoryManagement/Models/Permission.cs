using System.ComponentModel.DataAnnotations;

namespace SaaSInventoryManagement.Models
{
    public class Permission
    {
        public int Id { get; set; }

        [Required]
        public string Key { get; set; }

        [Required]
        public string DisplayName { get; set; }

        [Required]
        public string GroupName { get; set; }

        public string IconClass { get; set; }

        public string ControllerAction { get; set; }

        public int SortOrder { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
