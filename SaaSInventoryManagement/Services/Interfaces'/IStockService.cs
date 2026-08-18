using SaaSInventoryManagement.Enums;

namespace SaaSInventoryManagement.Services.Interfaces_
{
    public interface IStockService
    {
        Task<decimal> GetQuantityAsync(int productId, int warehouseId);

        Task AdjustAsync(int productId, int warehouseId, decimal quantityChange, string? notes);

        Task TransferAsync(int productId, int fromWarehouseId, int toWarehouseId, decimal quantity, string? notes);

        Task ReceiveAsync(int productId, int warehouseId, decimal quantity, StockMovementType movementType, string referenceType, int? referenceId, string? notes);

        Task IssueAsync(int productId, int warehouseId, decimal quantity, StockMovementType movementType, string referenceType, int? referenceId, string? notes);
    }
}
