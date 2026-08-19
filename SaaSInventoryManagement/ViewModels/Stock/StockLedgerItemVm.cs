using SaaSInventoryManagement.Enums;

namespace SaaSInventoryManagement.ViewModels.Stock
{
    public class StockLedgerItemVm
    {
        public string ProductName { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;
        public string WarehouseName { get; set; } = string.Empty;
        public StockMovementType MovementType { get; set; }
        public decimal QuantityChange { get; set; }
        public decimal BalanceAfter { get; set; }
        public string ReferenceType { get; set; } = string.Empty;
        public int? ReferenceId { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
    }
}
