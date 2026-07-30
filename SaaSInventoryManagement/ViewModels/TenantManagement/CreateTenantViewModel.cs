using System.ComponentModel.DataAnnotations;

namespace SaaSInventoryManagement.ViewModels.TenantManagement
{
    public class CreateTenantViewModel
    {
        [Required, Display(Name = "Company / Tenant name")]
        public string Name { get; set; }

        [Required]
        [RegularExpression("^[a-z0-9]+(-[a-z0-9]+)*$", ErrorMessage = "Slug may only contain lowercase letters, numbers and hyphens.")]
        public string Slug { get; set; }

        [Required, Display(Name = "Admin first name")]
        public string AdminFirstName { get; set; }

        [Required, Display(Name = "Admin last name")]
        public string AdminLastName { get; set; }

        [Required, EmailAddress, Display(Name = "Admin email")]
        public string AdminEmail { get; set; }

        [Required, DataType(DataType.Password), Display(Name = "Admin password")]
        public string AdminPassword { get; set; }
    }
}
