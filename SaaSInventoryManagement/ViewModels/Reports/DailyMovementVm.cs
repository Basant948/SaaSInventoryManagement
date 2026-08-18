namespace SaaSInventoryManagement.ViewModels.Reports
{
    public class DailyMovementVm
    {
        public DateTime Date { get; set; }
        public decimal QuantityIn { get; set; }
        public decimal QuantityOut { get; set; }
    }
}
