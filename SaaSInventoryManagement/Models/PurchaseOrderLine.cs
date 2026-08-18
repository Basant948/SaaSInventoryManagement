using SaaSInventoryManagement.Models.Base;
using System.ComponentModel.DataAnnotations.Schema;

namespace SaaSInventoryManagement.Models
{
    public class PurchaseOrderLine : IEntity, ITenantOwned
    {
        public int Id { get; set; }

        public int PurchaseOrderId { get; set; }
        [ForeignKey(nameof(PurchaseOrderId))]
        public PurchaseOrder? PurchaseOrder { get; set; }

        public int ProductId { get; set; }
        [ForeignKey(nameof(ProductId))]
        public Product? Product { get; set; }

        [Column(TypeName = "decimal(18,3)")]
        public decimal QuantityOrdered { get; set; }

        [Column(TypeName = "decimal(18,3)")]
        public decimal QuantityReceived { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitCost { get; set; }

        public int TenantId { get; set; }
    }
}
