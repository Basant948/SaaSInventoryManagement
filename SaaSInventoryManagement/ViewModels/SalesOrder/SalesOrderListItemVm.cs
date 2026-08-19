using SaaSInventoryManagement.Enums;

namespace SaaSInventoryManagement.ViewModels.SalesOrder
{
    public class SalesOrderListItemVm
    {
        public int Id { get; set; }
        public string SoNumber { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string WarehouseName { get; set; } = string.Empty;
        public SalesOrderStatus Status { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime? RequestedDate { get; set; }
        public decimal TotalAmount { get; set; }
    }
}
