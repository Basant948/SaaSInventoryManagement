using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Moq;
using SaaSInventoryManagement.Enums;
using SaaSInventoryManagement.Exceptions;
using SaaSInventoryManagement.Models;
using SaaSInventoryManagement.Repositories.Interfaces;
using SaaSInventoryManagement.Services;
using SaaSInventoryManagement.Services.Interfaces_;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using Xunit;

namespace SaasInventoryManagement.Test.Stock
{
    public class StockServiceTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWork = new();
        private readonly Mock<IGenericRepository<StockLevel>> _stockLevels = new();
        private readonly Mock<IGenericRepository<StockLedgerEntry>> _ledgerEntries = new();
        private readonly Mock<ICurrentUserService> _currentUser = new();
        private readonly StockService _stockService;

        public StockServiceTests()
        {
            var tenantProvider = new Mock<ITenantProvider>();
            tenantProvider.Setup(t => t.TenantId).Returns(1);

            var options = new DbContextOptionsBuilder<ApplicationDbContext>().Options;
            var dbContext = new ApplicationDbContext(options, tenantProvider.Object, _currentUser.Object);

            _currentUser.Setup(u => u.Username).Returns("test.user");

            _unitOfWork.Setup(u => u.Repository<StockLevel>()).Returns(_stockLevels.Object);
            _unitOfWork.Setup(u => u.Repository<StockLedgerEntry>()).Returns(_ledgerEntries.Object);
            _unitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

            _stockService = new StockService(_unitOfWork.Object, _currentUser.Object, dbContext);
        }

        [Fact]
        public async Task AdjustAsync_IncreasesQuantity_WhenChangeIsPositive()
        {
            var existingLevel = new StockLevel { ProductId = 1, WarehouseId = 1, Quantity = 20m };
            _stockLevels
                .Setup(r => r.FirstOrDefaultAsync(It.IsAny<Expression<Func<StockLevel, bool>>>()))
                .ReturnsAsync(existingLevel);

            await _stockService.AdjustAsync(productId: 1, warehouseId: 1, quantityChange: 5m, notes: "Stock count correction");

            Assert.Equal(25m, existingLevel.Quantity);
            _unitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task AdjustAsync_DecreasesQuantity_WhenChangeIsNegative()
        {
            var existingLevel = new StockLevel { ProductId = 1, WarehouseId = 1, Quantity = 20m };
            _stockLevels
                .Setup(r => r.FirstOrDefaultAsync(It.IsAny<Expression<Func<StockLevel, bool>>>()))
                .ReturnsAsync(existingLevel);

            await _stockService.AdjustAsync(productId: 1, warehouseId: 1, quantityChange: -8m, notes: "Damaged product write-off");

            Assert.Equal(12m, existingLevel.Quantity);
        }

        [Fact]
        public async Task AdjustAsync_ThrowsBadRequestException_WhenResultWouldBeNegative()
        {
            var existingLevel = new StockLevel { ProductId = 1, WarehouseId = 1, Quantity = 20m };
            _stockLevels
                .Setup(r => r.FirstOrDefaultAsync(It.IsAny<Expression<Func<StockLevel, bool>>>()))
                .ReturnsAsync(existingLevel);

            await Assert.ThrowsAsync<BadRequestException>(
                () => _stockService.AdjustAsync(productId: 1, warehouseId: 1, quantityChange: -999m, notes: "Too much"));
        }

        [Fact]
        public async Task AdjustAsync_ThrowsBadRequestException_WhenQuantityChangeIsZero()
        {
            await Assert.ThrowsAsync<BadRequestException>(
                () => _stockService.AdjustAsync(productId: 1, warehouseId: 1, quantityChange: 0m, notes: "No-op"));
        }

        [Fact]
        public async Task IssueAsync_ThrowsBadRequestException_WhenStockIsInsufficient()
        {

            var existingLevel = new StockLevel { ProductId = 1, WarehouseId = 1, Quantity = 9m };
            _stockLevels
                .Setup(r => r.FirstOrDefaultAsync(It.IsAny<Expression<Func<StockLevel, bool>>>()))
                .ReturnsAsync(existingLevel);

            var exception = await Assert.ThrowsAsync<BadRequestException>(() =>
                _stockService.IssueAsync(
                    productId: 1, warehouseId: 1, quantity: 15m,
                    movementType: StockMovementType.SalesIssue,
                    referenceType: "SalesOrder", referenceId: 100, notes: null));

            Assert.Contains("Insufficient stock", exception.Message);
        }

        [Fact]
        public async Task IssueAsync_DecreasesQuantity_WhenStockIsSufficient()
        {
            var existingLevel = new StockLevel { ProductId = 1, WarehouseId = 1, Quantity = 20m };
            _stockLevels
                .Setup(r => r.FirstOrDefaultAsync(It.IsAny<Expression<Func<StockLevel, bool>>>()))
                .ReturnsAsync(existingLevel);

            await _stockService.IssueAsync(
                productId: 1, warehouseId: 1, quantity: 4m,
                movementType: StockMovementType.SalesIssue,
                referenceType: "SalesOrder", referenceId: 100, notes: "SO-1001");

            Assert.Equal(16m, existingLevel.Quantity);
        }

        [Fact]
        public async Task ReceiveAsync_IncreasesQuantity()
        {
            var existingLevel = new StockLevel { ProductId = 1, WarehouseId = 1, Quantity = 20m };
            _stockLevels
                .Setup(r => r.FirstOrDefaultAsync(It.IsAny<Expression<Func<StockLevel, bool>>>()))
                .ReturnsAsync(existingLevel);

            await _stockService.ReceiveAsync(
                productId: 1, warehouseId: 1, quantity: 10m,
                movementType: StockMovementType.PurchaseReceipt,
                referenceType: "PurchaseOrder", referenceId: 200, notes: "PO-2001");

            Assert.Equal(30m, existingLevel.Quantity);
        }

        [Fact]
        public async Task GetQuantityAsync_ReturnsZero_WhenNoStockLevelExists()
        {
            _stockLevels
                .Setup(r => r.FirstOrDefaultAsync(It.IsAny<Expression<Func<StockLevel, bool>>>()))
                .ReturnsAsync((StockLevel?)null);

            var quantity = await _stockService.GetQuantityAsync(productId: 1, warehouseId: 1);

            Assert.Equal(0m, quantity);
        }
    }
}



