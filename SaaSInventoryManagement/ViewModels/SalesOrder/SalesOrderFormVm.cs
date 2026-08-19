using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace SaaSInventoryManagement.ViewModels.SalesOrder
{
    public class SalesOrderFormVm
    {
        [Required, Display(Name = "Customer")]
        public int CustomerId { get; set; }

        [Required, Display(Name = "Warehouse")]
        public int WarehouseId { get; set; }

        [Display(Name = "Requested Date")]
        [DataType(DataType.Date)]
        public DateTime? RequestedDate { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }

        public List<SalesOrderLineFormVm> Lines { get; set; } = new() { new SalesOrderLineFormVm() };

        public List<SelectListItem> Customers { get; set; } = new();
        public List<SelectListItem> Warehouses { get; set; } = new();
        public List<SelectListItem> Products { get; set; } = new();

        public Dictionary<int, decimal> ProductPrices { get; set; } = new();
    }
}
