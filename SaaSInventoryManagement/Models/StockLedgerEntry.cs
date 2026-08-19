using SaaSInventoryManagement.Enums;
using SaaSInventoryManagement.Models.Base;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SaaSInventoryManagement.Models
{
    public class StockLedgerEntry : IEntity, ITenantOwned, IAppendOnly
    {
        public int Id { get; set; }

        public int ProductId { get; set; }
        [ForeignKey(nameof(ProductId))]
        public Product? Product { get; set; }

        public int WarehouseId { get; set; }
        [ForeignKey(nameof(WarehouseId))]
        public Warehouse? Warehouse { get; set; }

        public StockMovementType MovementType { get; set; }

        [Column(TypeName = "decimal(18,3)")]
        public decimal QuantityChange { get; set; }

        [Column(TypeName = "decimal(18,3)")]
        public decimal BalanceAfter { get; set; }

        [StringLength(50)]
        public string ReferenceType { get; set; } = string.Empty;

        public int? ReferenceId { get; set; }

        [StringLength(300)]
        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string? CreatedBy { get; set; }

        public int TenantId { get; set; }
    }
}
