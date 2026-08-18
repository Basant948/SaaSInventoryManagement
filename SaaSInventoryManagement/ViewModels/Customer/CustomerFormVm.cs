using System.ComponentModel.DataAnnotations;

namespace SaaSInventoryManagement.ViewModels.Customer
{
    public class CustomerFormVm
    {
        public int Id { get; set; }

        [Required, StringLength(150)]
        public string Name { get; set; } = string.Empty;

        [StringLength(150), Display(Name = "Contact Person")]
        public string? ContactPerson { get; set; }

        [StringLength(150), EmailAddress]
        public string? Email { get; set; }

        [StringLength(30), Phone]
        public string? Phone { get; set; }

        [StringLength(300)]
        public string? Address { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;
    }
}
