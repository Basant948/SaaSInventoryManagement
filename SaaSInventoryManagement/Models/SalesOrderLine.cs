using SaaSInventoryManagement.Models.Base;
using System.ComponentModel.DataAnnotations.Schema;

namespace SaaSInventoryManagement.Models
{
    public class SalesOrderLine : IEntity, ITenantOwned
    {
        public int Id { get; set; }

        public int SalesOrderId { get; set; }
        [ForeignKey(nameof(SalesOrderId))]
        public SalesOrder? SalesOrder { get; set; }

        public int ProductId { get; set; }
        [ForeignKey(nameof(ProductId))]
        public Product? Product { get; set; }

        [Column(TypeName = "decimal(18,3)")]
        public decimal QuantityOrdered { get; set; }

        [Column(TypeName = "decimal(18,3)")]
        public decimal QuantityFulfilled { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }

        public int TenantId { get; set; }
    }
}
