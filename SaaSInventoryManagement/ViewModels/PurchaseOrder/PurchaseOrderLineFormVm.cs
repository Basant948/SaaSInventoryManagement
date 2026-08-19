namespace SaaSInventoryManagement.ViewModels.PurchaseOrder
{
    public class PurchaseOrderLineFormVm
    {
        public int ProductId { get; set; }

        public decimal QuantityOrdered { get; set; }

        public decimal UnitCost { get; set; }
    }
}
