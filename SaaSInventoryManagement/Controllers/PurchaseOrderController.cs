using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SaaSInventoryManagement.Enums;
using SaaSInventoryManagement.Exceptions;
using SaaSInventoryManagement.Repositories.Interfaces;
using SaaSInventoryManagement.Services.Interfaces_;
using SaaSInventoryManagement.ViewModels.PurchaseOrder;
using Microsoft.EntityFrameworkCore;
using PurchaseOrderModel = SaaSInventoryManagement.Models.PurchaseOrder;
using PurchaseOrderLineModel = SaaSInventoryManagement.Models.PurchaseOrderLine;

namespace SaaSInventoryManagement.Controllers
{
    [Authorize(Policy = "perm:inv.purchaseorders.view")]
    public class PurchaseOrderController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IStockService _stockService;

        public PurchaseOrderController(IUnitOfWork unitOfWork, IStockService stockService)
        {
            _unitOfWork = unitOfWork;
            _stockService = stockService;
        }

        public async Task<IActionResult> Index()
        {
            var orders = await _unitOfWork.PurchaseOrders.Query()
                .Include(po => po.Supplier)
                .Include(po => po.Warehouse)
                .Include(po => po.Lines)
                .OrderByDescending(po => po.OrderDate)
                .Select(po => new PurchaseOrderListItemVm
                {
                    Id = po.Id,
                    PoNumber = po.PoNumber,
                    SupplierName = po.Supplier != null ? po.Supplier.Name : string.Empty,
                    WarehouseName = po.Warehouse != null ? po.Warehouse.Name : string.Empty,
                    Status = po.Status,
                    OrderDate = po.OrderDate,
                    ExpectedDate = po.ExpectedDate,
                    TotalAmount = po.Lines.Sum(l => l.QuantityOrdered * l.UnitCost)
                })
                .ToListAsync();

            return View(orders);
        }

        public async Task<IActionResult> Details(int id)
        {
            var po = await _unitOfWork.PurchaseOrders.GetWithLinesAsync(id);
            if (po is null)
                return NotFound();

            return View(ToDetailsVm(po));
        }

        [Authorize(Policy = "perm:inv.purchaseorders.create")]
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var vm = new PurchaseOrderFormVm();
            await PopulateSelectListsAsync(vm);
            return View(vm);
        }

        [Authorize(Policy = "perm:inv.purchaseorders.create")]
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PurchaseOrderFormVm model)
        {
            ValidateLines(model);

            if (!ModelState.IsValid)
            {
                await PopulateSelectListsAsync(model);
                return View(model);
            }

            var po = new PurchaseOrderModel
            {
                PoNumber = await GenerateUniquePoNumberAsync(),
                SupplierId = model.SupplierId,
                WarehouseId = model.WarehouseId,
                ExpectedDate = model.ExpectedDate,
                Notes = string.IsNullOrWhiteSpace(model.Notes) ? null : model.Notes.Trim(),
                Status = PurchaseOrderStatus.Draft
            };

            foreach (var line in ValidLines(model))
            {
                po.Lines.Add(new PurchaseOrderLineModel
                {
                    ProductId = line.ProductId,
                    QuantityOrdered = line.QuantityOrdered,
                    UnitCost = line.UnitCost
                });
            }

            await _unitOfWork.PurchaseOrders.AddAsync(po);
            await _unitOfWork.SaveChangesAsync();

            TempData["Success"] = $"Purchase Order \"{po.PoNumber}\" created as Draft.";
            return RedirectToAction(nameof(Details), new { id = po.Id });
        }

        [Authorize(Policy = "perm:inv.purchaseorders.create")]
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(int id)
        {
            var po = await RequireStatusAsync(id, PurchaseOrderStatus.Draft, "submitted");
            if (po is null) return RedirectToAction(nameof(Details), new { id });

            po.Status = PurchaseOrderStatus.Submitted;
            po.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.PurchaseOrders.Update(po);
            await _unitOfWork.SaveChangesAsync();

            TempData["Success"] = $"Purchase Order \"{po.PoNumber}\" submitted.";
            return RedirectToAction(nameof(Details), new { id });
        }

        [Authorize(Policy = "perm:inv.purchaseorders.approve")]
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            var po = await RequireStatusAsync(id, PurchaseOrderStatus.Submitted, "approved");
            if (po is null) return RedirectToAction(nameof(Details), new { id });

            po.Status = PurchaseOrderStatus.Approved;
            po.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.PurchaseOrders.Update(po);
            await _unitOfWork.SaveChangesAsync();

            TempData["Success"] = $"Purchase Order \"{po.PoNumber}\" approved.";
            return RedirectToAction(nameof(Details), new { id });
        }

        [Authorize(Policy = "perm:inv.purchaseorders.approve")]
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Receive(int id)
        {
            var po = await _unitOfWork.PurchaseOrders.GetWithLinesAsync(id);
            if (po is null)
                return NotFound();

            if (po.Status != PurchaseOrderStatus.Approved)
            {
                TempData["Error"] = $"Only Approved orders can be received (current status: {po.Status}).";
                return RedirectToAction(nameof(Details), new { id });
            }

            foreach (var line in po.Lines)
            {
                var outstanding = line.QuantityOrdered - line.QuantityReceived;
                if (outstanding <= 0)
                    continue;

                await _stockService.ReceiveAsync(
                    line.ProductId, po.WarehouseId, outstanding,
                    StockMovementType.PurchaseReceipt, "PurchaseOrder", po.Id,
                    $"Received against PO {po.PoNumber}");

                line.QuantityReceived = line.QuantityOrdered;
            }

            po.Status = PurchaseOrderStatus.Received;
            po.ReceivedAt = DateTime.UtcNow;
            po.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.PurchaseOrders.Update(po);
            await _unitOfWork.SaveChangesAsync();

            TempData["Success"] = $"Purchase Order \"{po.PoNumber}\" received - stock levels updated.";
            return RedirectToAction(nameof(Details), new { id });
        }

        [Authorize(Policy = "perm:inv.purchaseorders.create")]
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var po = await _unitOfWork.PurchaseOrders.GetByIdAsync(id);
            if (po is null)
                return NotFound();

            if (po.Status is not (PurchaseOrderStatus.Draft or PurchaseOrderStatus.Submitted))
            {
                TempData["Error"] = $"Only Draft or Submitted orders can be cancelled (current status: {po.Status}).";
                return RedirectToAction(nameof(Details), new { id });
            }

            po.Status = PurchaseOrderStatus.Cancelled;
            po.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.PurchaseOrders.Update(po);
            await _unitOfWork.SaveChangesAsync();

            TempData["Success"] = $"Purchase Order \"{po.PoNumber}\" cancelled.";
            return RedirectToAction(nameof(Details), new { id });
        }


        private async Task<PurchaseOrderModel?> RequireStatusAsync(int id, PurchaseOrderStatus requiredStatus, string actionPastTense)
        {
            var po = await _unitOfWork.PurchaseOrders.GetByIdAsync(id);
            if (po is null)
                throw new NotFoundException($"Purchase Order {id} was not found.");

            if (po.Status != requiredStatus)
            {
                TempData["Error"] = $"Only {requiredStatus} orders can be {actionPastTense} (current status: {po.Status}).";
                return null;
            }

            return po;
        }

        private static IEnumerable<PurchaseOrderLineFormVm> ValidLines(PurchaseOrderFormVm model) =>
            model.Lines.Where(l => l.ProductId > 0 && l.QuantityOrdered > 0);

        private void ValidateLines(PurchaseOrderFormVm model)
        {
            var intendedLines = model.Lines.Where(l => l.ProductId > 0).ToList();

            if (intendedLines.Count == 0)
            {
                ModelState.AddModelError(string.Empty, "Add at least one line with a product and quantity.");
                return;
            }

            for (var i = 0; i < intendedLines.Count; i++)
            {
                if (intendedLines[i].QuantityOrdered <= 0)
                    ModelState.AddModelError(string.Empty, $"Line {i + 1}: quantity must be greater than zero.");
                if (intendedLines[i].UnitCost < 0)
                    ModelState.AddModelError(string.Empty, $"Line {i + 1}: unit cost cannot be negative.");
            }
        }

        private async Task<string> GenerateUniquePoNumberAsync()
        {
            var prefix = $"PO-{DateTime.UtcNow:yyyyMMdd}-";
            for (var attempt = 0; attempt < 5; attempt++)
            {
                var candidate = prefix + Random.Shared.Next(1000, 9999);
                if (!await _unitOfWork.PurchaseOrders.PoNumberExistsAsync(candidate))
                    return candidate;
            }

            return prefix + Guid.NewGuid().ToString("N")[..6];
        }

        private async Task PopulateSelectListsAsync(PurchaseOrderFormVm model)
        {
            model.Suppliers = await _unitOfWork.Suppliers.Query()
                .Where(s => s.IsActive)
                .OrderBy(s => s.Name)
                .Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.Name })
                .ToListAsync();

            model.Warehouses = await _unitOfWork.Warehouses.Query()
                .Where(w => w.IsActive)
                .OrderBy(w => w.Name)
                .Select(w => new SelectListItem { Value = w.Id.ToString(), Text = w.Name })
                .ToListAsync();

            var products = await _unitOfWork.Products.Query()
                .Where(p => p.IsActive)
                .OrderBy(p => p.Name)
                .Select(p => new { p.Id, p.Name, p.SKU, p.CostPrice })
                .ToListAsync();

            model.Products = products
                .Select(p => new SelectListItem { Value = p.Id.ToString(), Text = p.Name + " (" + p.SKU + ")" })
                .ToList();

            model.ProductCosts = products.ToDictionary(p => p.Id, p => p.CostPrice);
        }

        private static PurchaseOrderDetailsVm ToDetailsVm(PurchaseOrderModel po) => new()
        {
            Id = po.Id,
            PoNumber = po.PoNumber,
            SupplierName = po.Supplier?.Name ?? string.Empty,
            WarehouseName = po.Warehouse?.Name ?? string.Empty,
            Status = po.Status,
            OrderDate = po.OrderDate,
            ExpectedDate = po.ExpectedDate,
            ReceivedAt = po.ReceivedAt,
            Notes = po.Notes,
            Lines = po.Lines.Select(l => new PurchaseOrderLineDetailVm
            {
                ProductName = l.Product?.Name ?? string.Empty,
                SKU = l.Product?.SKU ?? string.Empty,
                Unit = l.Product?.Unit ?? string.Empty,
                QuantityOrdered = l.QuantityOrdered,
                QuantityReceived = l.QuantityReceived,
                UnitCost = l.UnitCost
            }).ToList()
        };
    }
}
