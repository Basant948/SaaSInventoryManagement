namespace SaaSInventoryManagement.ViewModels.SalesOrder
{
    public class SalesOrderLineFormVm
    {
        public int ProductId { get; set; }
        public decimal QuantityOrdered { get; set; }
        public decimal UnitPrice { get; set; }
    }
}
