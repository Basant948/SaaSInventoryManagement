using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SaaSInventoryManagement.Models;
using SaaSInventoryManagement.Repositories.Interfaces;
using SaaSInventoryManagement.Services.Interfaces_;
using SaaSInventoryManagement.ViewModels.Reports;
using Microsoft.EntityFrameworkCore;

namespace SaaSInventoryManagement.Controllers
{
    [Authorize(Policy = "perm:inv.reports.stock")]
    public class ReportController : Controller
    {
        private const int MovementWindowDays = 30;
        private const int LowStockItemCount = 10;

        private readonly IUnitOfWork _unitOfWork;
        private readonly ApplicationDbContext _db;
        private readonly ITenantProvider _tenantProvider;

        public ReportController(IUnitOfWork unitOfWork, ApplicationDbContext db, ITenantProvider tenantProvider)
        {
            _unitOfWork = unitOfWork;
            _db = db;
            _tenantProvider = tenantProvider;
        }

        public async Task<IActionResult> Stock()
        {

            var lowStockThreshold = _tenantProvider.TenantId is int tenantId
                ? await _db.Tenants.Where(t => t.Id == tenantId).Select(t => t.LowStockThreshold).FirstOrDefaultAsync()
                : 10m;

            var levelQuery = _unitOfWork.Repository<StockLevel>().Query();

            var totalProducts = await levelQuery.Select(s => s.ProductId).Distinct().CountAsync();
            var totalWarehouses = await levelQuery.Select(s => s.WarehouseId).Distinct().CountAsync();
            var totalOnHandQuantity = await levelQuery.SumAsync(s => (decimal?)s.Quantity) ?? 0m;
            var outOfStockCount = await levelQuery.CountAsync(s => s.Quantity <= 0);
            var belowThresholdCount = await levelQuery.CountAsync(s => s.Quantity <= lowStockThreshold);

            var byWarehouse = await levelQuery
                .GroupBy(s => s.Warehouse!.Name)
                .Select(g => new WarehouseStockSummaryVm
                {
                    WarehouseName = g.Key,
                    TotalQuantity = g.Sum(s => s.Quantity)
                })
                .OrderByDescending(w => w.TotalQuantity)
                .ToListAsync();

            var lowestStockItems = await levelQuery
                .Include(s => s.Product)
                .Include(s => s.Warehouse)
                .OrderBy(s => s.Quantity)
                .Take(LowStockItemCount)
                .Select(s => new LowStockItemVm
                {
                    ProductName = s.Product!.Name,
                    SKU = s.Product.SKU,
                    WarehouseName = s.Warehouse!.Name,
                    Quantity = s.Quantity,
                    IsBelowThreshold = s.Quantity <= lowStockThreshold
                })
                .ToListAsync();

            var since = DateTime.UtcNow.Date.AddDays(-(MovementWindowDays - 1));

            var movementTotalsByDay = await _unitOfWork.Repository<StockLedgerEntry>().Query()
                .Where(l => l.CreatedAt >= since)
                .GroupBy(l => l.CreatedAt.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    QuantityIn = g.Where(l => l.QuantityChange > 0).Sum(l => (decimal?)l.QuantityChange) ?? 0m,
                    QuantityOut = g.Where(l => l.QuantityChange < 0).Sum(l => (decimal?)l.QuantityChange) ?? 0m
                })
                .ToDictionaryAsync(x => x.Date, x => x);

            var dailyMovements = Enumerable.Range(0, MovementWindowDays)
                .Select(offset => since.AddDays(offset))
                .Select(day => movementTotalsByDay.TryGetValue(day, out var totals)
                    ? new DailyMovementVm { Date = day, QuantityIn = totals.QuantityIn, QuantityOut = -totals.QuantityOut }
                    : new DailyMovementVm { Date = day, QuantityIn = 0m, QuantityOut = 0m })
                .ToList();

            return View(new StockReportVm
            {
                TotalProducts = totalProducts,
                TotalWarehouses = totalWarehouses,
                TotalOnHandQuantity = totalOnHandQuantity,
                OutOfStockCount = outOfStockCount,
                LowStockThreshold = lowStockThreshold,
                BelowThresholdCount = belowThresholdCount,
                ByWarehouse = byWarehouse,
                DailyMovements = dailyMovements,
                LowestStockItems = lowestStockItems
            });
        }
    }
}
