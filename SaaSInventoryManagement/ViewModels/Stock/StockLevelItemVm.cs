namespace SaaSInventoryManagement.ViewModels.Stock
{
    public class StockLevelItemVm
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;
        public string Unit { get; set; } = string.Empty;
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
    }
}
