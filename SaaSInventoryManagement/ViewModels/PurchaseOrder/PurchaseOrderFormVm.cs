using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace SaaSInventoryManagement.ViewModels.PurchaseOrder
{
    public class PurchaseOrderFormVm
    {
        [Required, Display(Name = "Supplier")]
        public int SupplierId { get; set; }

        [Required, Display(Name = "Warehouse")]
        public int WarehouseId { get; set; }

        [Display(Name = "Expected Date")]
        [DataType(DataType.Date)]
        public DateTime? ExpectedDate { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }

        public List<PurchaseOrderLineFormVm> Lines { get; set; } = new() { new PurchaseOrderLineFormVm() };

        public List<SelectListItem> Suppliers { get; set; } = new();
        public List<SelectListItem> Warehouses { get; set; } = new();
        public List<SelectListItem> Products { get; set; } = new();

        public Dictionary<int, decimal> ProductCosts { get; set; } = new();
    }
}
