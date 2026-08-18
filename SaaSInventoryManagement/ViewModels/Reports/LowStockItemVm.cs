namespace SaaSInventoryManagement.ViewModels.Reports
{
    public class LowStockItemVm
    {
        public string ProductName { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;
        public string WarehouseName { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public bool IsBelowThreshold { get; set; }
    }
}
