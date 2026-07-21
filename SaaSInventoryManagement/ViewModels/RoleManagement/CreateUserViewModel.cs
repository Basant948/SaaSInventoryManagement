using System.ComponentModel.DataAnnotations;

namespace SaaSInventoryManagement.ViewModels.RoleManagement
{
    public class CreateUserViewModel
    {
        [Required, Display(Name = "First name")]
        public string FirstName { get; set; }

        [Required, Display(Name = "Last name")]
        public string LastName { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        [Required, DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
