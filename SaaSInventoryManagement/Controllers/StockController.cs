using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SaaSInventoryManagement.Models;
using SaaSInventoryManagement.Repositories.Interfaces;
using SaaSInventoryManagement.Services.Interfaces_;
using SaaSInventoryManagement.ViewModels.Stock;
using Microsoft.EntityFrameworkCore;

namespace SaaSInventoryManagement.Controllers
{
    [Authorize(Policy = "perm:inv.stock.view")]
    public class StockController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IStockService _stockService;

        public StockController(IUnitOfWork unitOfWork, IStockService stockService)
        {
            _unitOfWork = unitOfWork;
            _stockService = stockService;
        }

        public async Task<IActionResult> Index(int? warehouseId)
        {
            var query = _unitOfWork.Repository<StockLevel>().Query()
                .Include(s => s.Product)
                .Include(s => s.Warehouse)
                .AsQueryable();

            if (warehouseId.HasValue)
                query = query.Where(s => s.WarehouseId == warehouseId.Value);

            var levels = await query
                .OrderBy(s => s.Product!.Name)
                .Select(s => new StockLevelItemVm
                {
                    ProductId = s.ProductId,
                    ProductName = s.Product!.Name,
                    SKU = s.Product.SKU,
                    Unit = s.Product.Unit,
                    WarehouseId = s.WarehouseId,
                    WarehouseName = s.Warehouse!.Name,
                    Quantity = s.Quantity
                })
                .ToListAsync();

            ViewBag.Warehouses = await GetWarehouseSelectListAsync();
            ViewBag.SelectedWarehouseId = warehouseId;
            return View(levels);
        }

        public async Task<IActionResult> History(int? productId, int? warehouseId)
        {
            var query = _unitOfWork.Repository<StockLedgerEntry>().Query()
                .Include(l => l.Product)
                .Include(l => l.Warehouse)
                .AsQueryable();

            if (productId.HasValue)
                query = query.Where(l => l.ProductId == productId.Value);
            if (warehouseId.HasValue)
                query = query.Where(l => l.WarehouseId == warehouseId.Value);

            var entries = await query
                .OrderByDescending(l => l.CreatedAt)
                .Take(200)
                .Select(l => new StockLedgerItemVm
                {
                    ProductName = l.Product!.Name,
                    SKU = l.Product.SKU,
                    WarehouseName = l.Warehouse!.Name,
                    MovementType = l.MovementType,
                    QuantityChange = l.QuantityChange,
                    BalanceAfter = l.BalanceAfter,
                    ReferenceType = l.ReferenceType,
                    ReferenceId = l.ReferenceId,
                    Notes = l.Notes,
                    CreatedAt = l.CreatedAt,
                    CreatedBy = l.CreatedBy
                })
                .ToListAsync();

            return View(entries);
        }

        [Authorize(Policy = "perm:inv.stock.adjust")]
        [HttpGet]
        public async Task<IActionResult> Adjust()
        {
            var vm = new StockAdjustFormVm
            {
                Products = await GetProductSelectListAsync(),
                Warehouses = await GetWarehouseSelectListAsync()
            };
            return View(vm);
        }

        [Authorize(Policy = "perm:inv.stock.adjust")]
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Adjust(StockAdjustFormVm model)
        {
            if (!ModelState.IsValid)
            {
                model.Products = await GetProductSelectListAsync();
                model.Warehouses = await GetWarehouseSelectListAsync();
                return View(model);
            }

            var notes = string.IsNullOrWhiteSpace(model.Notes)
                ? $"[{model.AdjustmentReason}]"
                : $"[{model.AdjustmentReason}] {model.Notes}";
            await _stockService.AdjustAsync(model.ProductId, model.WarehouseId, model.QuantityChange, notes);

            TempData["Success"] = "Stock adjustment recorded.";
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Policy = "perm:inv.stock.transfer")]
        [HttpGet]
        public async Task<IActionResult> Transfer()
        {
            var vm = new StockTransferFormVm
            {
                Products = await GetProductSelectListAsync(),
                Warehouses = await GetWarehouseSelectListAsync()
            };
            return View(vm);
        }

        [Authorize(Policy = "perm:inv.stock.transfer")]
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Transfer(StockTransferFormVm model)
        {
            if (model.FromWarehouseId == model.ToWarehouseId)
                ModelState.AddModelError(nameof(model.ToWarehouseId), "Source and destination warehouse must be different.");

            if (!ModelState.IsValid)
            {
                model.Products = await GetProductSelectListAsync();
                model.Warehouses = await GetWarehouseSelectListAsync();
                return View(model);
            }

            await _stockService.TransferAsync(model.ProductId, model.FromWarehouseId, model.ToWarehouseId, model.Quantity, model.Notes);

            TempData["Success"] = "Stock transfer completed.";
            return RedirectToAction(nameof(Index));
        }


        private async Task<List<SelectListItem>> GetProductSelectListAsync()
        {
            return await _unitOfWork.Products.Query()
                .Where(p => p.IsActive)
                .OrderBy(p => p.Name)
                .Select(p => new SelectListItem { Value = p.Id.ToString(), Text = p.Name + " (" + p.SKU + ")" })
                .ToListAsync();
        }

        private async Task<List<SelectListItem>> GetWarehouseSelectListAsync()
        {
            return await _unitOfWork.Warehouses.Query()
                .Where(w => w.IsActive)
                .OrderBy(w => w.Name)
                .Select(w => new SelectListItem { Value = w.Id.ToString(), Text = w.Name })
                .ToListAsync();
        }
    }
}
