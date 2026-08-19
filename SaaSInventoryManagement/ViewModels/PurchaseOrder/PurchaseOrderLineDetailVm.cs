namespace SaaSInventoryManagement.ViewModels.PurchaseOrder
{
    public class PurchaseOrderLineDetailVm
    {
        public string ProductName { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;
        public string Unit { get; set; } = string.Empty;
        public decimal QuantityOrdered { get; set; }
        public decimal QuantityReceived { get; set; }
        public decimal UnitCost { get; set; }
        public decimal LineTotal => QuantityOrdered * UnitCost;
    }
}
