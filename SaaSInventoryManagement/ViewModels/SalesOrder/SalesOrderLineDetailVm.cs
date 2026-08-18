namespace SaaSInventoryManagement.ViewModels.SalesOrder
{
    public class SalesOrderLineDetailVm
    {
        public string ProductName { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;
        public string Unit { get; set; } = string.Empty;
        public decimal QuantityOrdered { get; set; }
        public decimal QuantityFulfilled { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal LineTotal => QuantityOrdered * UnitPrice;
    }
}
