namespace SaaSInventoryManagement.ViewModels.Reports
{
    public class StockReportVm
    {
        public int TotalProducts { get; set; }
        public int TotalWarehouses { get; set; }
        public decimal TotalOnHandQuantity { get; set; }
        public int OutOfStockCount { get; set; }

        public decimal LowStockThreshold { get; set; }
        public int BelowThresholdCount { get; set; }

        public List<WarehouseStockSummaryVm> ByWarehouse { get; set; } = new();

        public List<DailyMovementVm> DailyMovements { get; set; } = new();

        public List<LowStockItemVm> LowestStockItems { get; set; } = new();
    }
}
