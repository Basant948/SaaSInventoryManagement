using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SaaSInventoryManagement.Models
{
    public class Tenant
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Slug { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        [StringLength(256)]
        public string? ContactEmail { get; set; }

        [StringLength(30)]
        public string? ContactPhone { get; set; }

        [StringLength(300)]
        public string? Address { get; set; }


        [Required, StringLength(3)]
        public string Currency { get; set; } = "USD";

        [Column(TypeName = "decimal(18,3)")]
        public decimal LowStockThreshold { get; set; } = 10m;
    }
}
