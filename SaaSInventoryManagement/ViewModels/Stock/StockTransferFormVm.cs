using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace SaaSInventoryManagement.ViewModels.Stock
{
    public class StockTransferFormVm
    {
        [Required, Display(Name = "Product")]
        public int ProductId { get; set; }

        [Required, Display(Name = "From Warehouse")]
        public int FromWarehouseId { get; set; }

        [Required, Display(Name = "To Warehouse")]
        public int ToWarehouseId { get; set; }

        [Required, Range(0.001, double.MaxValue, ErrorMessage = "Quantity must be greater than zero.")]
        public decimal Quantity { get; set; }

        [StringLength(300)]
        public string? Notes { get; set; }

        public List<SelectListItem> Products { get; set; } = new();
        public List<SelectListItem> Warehouses { get; set; } = new();
    }
}
