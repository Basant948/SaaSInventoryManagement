using System.ComponentModel.DataAnnotations;

namespace SaaSInventoryManagement.ViewModels.Warehouse
{
    public class WarehouseFormVm
    {
        public int Id { get; set; }

        [Required, StringLength(150)]
        public string Name { get; set; } = string.Empty;

        [Required, StringLength(20)]
        public string Code { get; set; } = string.Empty;

        [StringLength(300)]
        public string? Address { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;
    }
}
