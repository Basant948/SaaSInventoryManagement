using SaaSInventoryManagement.Models.Base;
using System.ComponentModel.DataAnnotations.Schema;

namespace SaaSInventoryManagement.Models
{
    public class StockLevel : IEntity, ITenantOwned
    {
        public int Id { get; set; }

        public int ProductId { get; set; }
        [ForeignKey(nameof(ProductId))]
        public Product? Product { get; set; }

        public int WarehouseId { get; set; }
        [ForeignKey(nameof(WarehouseId))]
        public Warehouse? Warehouse { get; set; }

        [Column(TypeName = "decimal(18,3)")]
        public decimal Quantity { get; set; }

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public byte[] RowVersion { get; set; } = Array.Empty<byte>();

        public int TenantId { get; set; }
    }
}
