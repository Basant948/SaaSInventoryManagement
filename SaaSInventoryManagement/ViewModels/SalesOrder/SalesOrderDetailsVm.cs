using SaaSInventoryManagement.Enums;

namespace SaaSInventoryManagement.ViewModels.SalesOrder
{
    public class SalesOrderDetailsVm
    {
        public int Id { get; set; }
        public string SoNumber { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string WarehouseName { get; set; } = string.Empty;
        public SalesOrderStatus Status { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime? RequestedDate { get; set; }
        public DateTime? FulfilledAt { get; set; }
        public string? Notes { get; set; }
        public List<SalesOrderLineDetailVm> Lines { get; set; } = new();
        public decimal TotalAmount => Lines.Sum(l => l.LineTotal);
    }
}
