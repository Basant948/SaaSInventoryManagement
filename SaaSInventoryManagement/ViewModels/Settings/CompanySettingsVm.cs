using System.ComponentModel.DataAnnotations;

namespace SaaSInventoryManagement.ViewModels.Settings
{
    public class CompanySettingsVm
    {
        [Required, StringLength(150), Display(Name = "Company Name")]
        public string Name { get; set; } = string.Empty;

        public string Slug { get; set; } = string.Empty;

        [StringLength(256), EmailAddress, Display(Name = "Contact Email")]
        public string? ContactEmail { get; set; }

        [StringLength(30), Phone, Display(Name = "Contact Phone")]
        public string? ContactPhone { get; set; }

        [StringLength(300)]
        public string? Address { get; set; }

        [Required, StringLength(3, MinimumLength = 3), Display(Name = "Currency (ISO 4217 code)")]
        public string Currency { get; set; } = "USD";

        [Required, Range(0, 1_000_000), Display(Name = "Low Stock Threshold")]
        public decimal LowStockThreshold { get; set; } = 10m;

        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
