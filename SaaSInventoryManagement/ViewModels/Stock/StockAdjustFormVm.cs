using Microsoft.AspNetCore.Mvc.Rendering;
using SaaSInventoryManagement.Enums;
using System.ComponentModel.DataAnnotations;

namespace SaaSInventoryManagement.ViewModels.Stock
{
    public class StockAdjustFormVm
    {
        [Required, Display(Name = "Product")]
        public int ProductId { get; set; }

        [Required, Display(Name = "Warehouse")]
        public int WarehouseId { get; set; }

        [Required, Display(Name = "Quantity Change")]
        public decimal QuantityChange { get; set; }

        [Required, Display(Name = "Reason")]
        public StockAdjustmentReason AdjustmentReason { get; set; }

        [StringLength(300), Display(Name = "Notes")]
        public string? Notes { get; set; }

        public List<SelectListItem> Products { get; set; } = new();
        public List<SelectListItem> Warehouses { get; set; } = new();
    }
}
