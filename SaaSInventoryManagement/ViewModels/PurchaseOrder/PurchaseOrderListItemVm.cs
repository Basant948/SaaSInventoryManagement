using SaaSInventoryManagement.Enums;

namespace SaaSInventoryManagement.ViewModels.PurchaseOrder
{
    public class PurchaseOrderListItemVm
    {
        public int Id { get; set; }
        public string PoNumber { get; set; } = string.Empty;
        public string SupplierName { get; set; } = string.Empty;
        public string WarehouseName { get; set; } = string.Empty;
        public PurchaseOrderStatus Status { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime? ExpectedDate { get; set; }
        public decimal TotalAmount { get; set; }
    }
}
