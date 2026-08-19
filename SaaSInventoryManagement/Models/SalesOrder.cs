using SaaSInventoryManagement.Enums;
using SaaSInventoryManagement.Models.Base;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SaaSInventoryManagement.Models
{
    public class SalesOrder : IEntity, ITenantOwned, ISoftDelete
    {
        public int Id { get; set; }

        [Required, StringLength(30)]
        public string SoNumber { get; set; } = string.Empty;

        public int CustomerId { get; set; }
        [ForeignKey(nameof(CustomerId))]
        public Customer? Customer { get; set; }

        public int WarehouseId { get; set; }
        [ForeignKey(nameof(WarehouseId))]
        public Warehouse? Warehouse { get; set; }

        public SalesOrderStatus Status { get; set; } = SalesOrderStatus.Draft;

        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        public DateTime? RequestedDate { get; set; }
        public DateTime? FulfilledAt { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }

        public List<SalesOrderLine> Lines { get; set; } = new();

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public int TenantId { get; set; }

        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
        public string? DeletedBy { get; set; }
    }
}
