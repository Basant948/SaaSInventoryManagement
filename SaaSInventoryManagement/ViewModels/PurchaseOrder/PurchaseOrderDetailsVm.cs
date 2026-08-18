using SaaSInventoryManagement.Enums;

namespace SaaSInventoryManagement.ViewModels.PurchaseOrder
{
    public class PurchaseOrderDetailsVm
    {
        public int Id { get; set; }
        public string PoNumber { get; set; } = string.Empty;
        public string SupplierName { get; set; } = string.Empty;
        public string WarehouseName { get; set; } = string.Empty;
        public PurchaseOrderStatus Status { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime? ExpectedDate { get; set; }
        public DateTime? ReceivedAt { get; set; }
        public string? Notes { get; set; }
        public List<PurchaseOrderLineDetailVm> Lines { get; set; } = new();
        public decimal TotalAmount => Lines.Sum(l => l.LineTotal);
    }
}
