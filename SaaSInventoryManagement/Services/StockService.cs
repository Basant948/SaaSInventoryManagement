using Microsoft.EntityFrameworkCore;
using SaaSInventoryManagement.Enums;
using SaaSInventoryManagement.Exceptions;
using SaaSInventoryManagement.Models;
using SaaSInventoryManagement.Repositories.Interfaces;
using SaaSInventoryManagement.Services.Interfaces_;

namespace SaaSInventoryManagement.Services
{
    public class StockService : IStockService
    {
        private const int MaxConcurrencyRetries = 3;

        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUser;

        private readonly ApplicationDbContext _db;

        public StockService(IUnitOfWork unitOfWork, ICurrentUserService currentUser, ApplicationDbContext db)
        {
            _unitOfWork = unitOfWork;
            _currentUser = currentUser;
            _db = db;
        }

        public async Task<decimal> GetQuantityAsync(int productId, int warehouseId)
        {
            var level = await _unitOfWork.Repository<StockLevel>()
                .FirstOrDefaultAsync(s => s.ProductId == productId && s.WarehouseId == warehouseId);

            return level?.Quantity ?? 0m;
        }

        public async Task AdjustAsync(int productId, int warehouseId, decimal quantityChange, string? notes)
        {
            if (quantityChange == 0)
                throw new BadRequestException("Adjustment quantity cannot be zero.");


            await ApplyMovementAndSaveWithRetryAsync(
                productId, warehouseId, quantityChange,
                StockMovementType.Adjustment, "ManualAdjustment", null, notes);
        }

        public async Task TransferAsync(int productId, int fromWarehouseId, int toWarehouseId, decimal quantity, string? notes)
        {
            if (quantity <= 0)
                throw new BadRequestException("Transfer quantity must be greater than zero.");

            if (fromWarehouseId == toWarehouseId)
                throw new BadRequestException("Source and destination warehouse must be different.");

            await using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var outEntry = await ApplyMovementAndSaveWithRetryAsync(
                    productId, fromWarehouseId, -quantity,
                    StockMovementType.TransferOut, "Transfer", null, notes);

                await ApplyMovementAndSaveWithRetryAsync(
                    productId, toWarehouseId, quantity,
                    StockMovementType.TransferIn, "Transfer", outEntry.Id, notes);

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task ReceiveAsync(int productId, int warehouseId, decimal quantity, StockMovementType movementType, string referenceType, int? referenceId, string? notes)
        {
            if (quantity <= 0)
                throw new BadRequestException("Receive quantity must be greater than zero.");

            await ApplyMovementAsync(productId, warehouseId, quantity, movementType, referenceType, referenceId, notes);
        }

        public async Task IssueAsync(int productId, int warehouseId, decimal quantity, StockMovementType movementType, string referenceType, int? referenceId, string? notes)
        {
            if (quantity <= 0)
                throw new BadRequestException("Issue quantity must be greater than zero.");


            await ApplyMovementAsync(productId, warehouseId, -quantity, movementType, referenceType, referenceId, notes);
        }


        private async Task<StockLedgerEntry> ApplyMovementAsync(
            int productId, int warehouseId, decimal quantityChange,
            StockMovementType movementType, string referenceType, int? referenceId, string? notes)
        {
            var levelRepo = _unitOfWork.Repository<StockLevel>();
            var level = await levelRepo.FirstOrDefaultAsync(
                s => s.ProductId == productId && s.WarehouseId == warehouseId);

            if (level is null)
            {
                level = new StockLevel { ProductId = productId, WarehouseId = warehouseId, Quantity = 0 };
                await levelRepo.AddAsync(level);
            }

            var ledgerEntry = new StockLedgerEntry
            {
                ProductId = productId,
                WarehouseId = warehouseId,
                MovementType = movementType,
                QuantityChange = quantityChange,
                ReferenceType = referenceType,
                ReferenceId = referenceId,
                Notes = notes,
                CreatedBy = _currentUser.Username
            };
            await _unitOfWork.Repository<StockLedgerEntry>().AddAsync(ledgerEntry);

            ApplyQuantityChange(level, ledgerEntry, quantityChange);
            levelRepo.Update(level);

            return ledgerEntry;
        }


        private static void ApplyQuantityChange(StockLevel level, StockLedgerEntry ledgerEntry, decimal quantityChange)
        {
            var newQuantity = level.Quantity + quantityChange;
            if (newQuantity < 0)
                throw new BadRequestException(
                    $"Insufficient stock. Available: {level.Quantity}, requested change: {quantityChange}.");

            level.Quantity = newQuantity;
            level.UpdatedAt = DateTime.UtcNow;
            ledgerEntry.BalanceAfter = newQuantity;
        }


        private async Task<StockLedgerEntry> ApplyMovementAndSaveWithRetryAsync(
            int productId, int warehouseId, decimal quantityChange,
            StockMovementType movementType, string referenceType, int? referenceId, string? notes)
        {
            var ledgerEntry = await ApplyMovementAsync(
                productId, warehouseId, quantityChange, movementType, referenceType, referenceId, notes);

            for (var attempt = 1; ; attempt++)
            {
                try
                {
                    await _unitOfWork.SaveChangesAsync();
                    return ledgerEntry;
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    if (attempt >= MaxConcurrencyRetries)
                        throw new ConflictException(
                            "This stock record was updated by someone else at the same time. Please try again.");


                    foreach (var staleAuditLog in _db.ChangeTracker.Entries<AuditLog>()
                                 .Where(e => e.State == EntityState.Added).ToList())
                    {
                        staleAuditLog.State = EntityState.Detached;
                    }

                    foreach (var conflictingEntry in ex.Entries)
                    {
                        if (conflictingEntry.Entity is StockLevel level)
                        {
                            await conflictingEntry.ReloadAsync();
                            ApplyQuantityChange(level, ledgerEntry, quantityChange);
                        }
                    }
                }
            }
        }
    }
}
