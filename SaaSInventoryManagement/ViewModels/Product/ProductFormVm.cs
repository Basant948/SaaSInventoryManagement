using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace SaaSInventoryManagement.ViewModels.Product
{
    public class ProductFormVm
    {
        public int Id { get; set; }

        [Required, StringLength(150)]
        public string Name { get; set; } = string.Empty;

        [Required, StringLength(50)]
        [Display(Name = "SKU")]
        public string SKU { get; set; } = string.Empty;

        [Required, Display(Name = "Category")]
        public int CategoryId { get; set; }

        [StringLength(1000)]
        public string? Description { get; set; }

        [Required, StringLength(20)]
        public string Unit { get; set; } = string.Empty;

        [Required, Range(0, double.MaxValue, ErrorMessage = "Cost price must be zero or greater.")]
        [Display(Name = "Cost Price")]
        public decimal CostPrice { get; set; }

        [Required, Range(0, double.MaxValue, ErrorMessage = "Selling price must be zero or greater.")]
        [Display(Name = "Selling Price")]
        public decimal SellingPrice { get; set; }

        [Display(Name = "Image")]
        public IFormFile? ImageFile { get; set; }

        public string? ExistingImage { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;

        public List<SelectListItem> Categories { get; set; } = new();
    }
}
