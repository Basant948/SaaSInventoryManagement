using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SaaSInventoryManagement.Enums;
using SaaSInventoryManagement.Exceptions;
using SaaSInventoryManagement.Repositories.Interfaces;
using SaaSInventoryManagement.Services.Interfaces_;
using SaaSInventoryManagement.ViewModels.SalesOrder;
using SalesOrderLineModel =SaaSInventoryManagement.Models.SalesOrderLine;
using SalesOrderModel = SaaSInventoryManagement.Models.SalesOrder;

namespace SaaSInventoryManagement.Controllers
{
    [Authorize(Policy = "perm:inv.salesorders.view")]
    public class SalesOrderController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IStockService _stockService;

        public SalesOrderController(IUnitOfWork unitOfWork, IStockService stockService)
        {
            _unitOfWork = unitOfWork;
            _stockService = stockService;
        }

        public async Task<IActionResult> Index()
        {
            var orders = await _unitOfWork.SalesOrders.Query()
                .Include(so => so.Customer)
                .Include(so => so.Warehouse)
                .Include(so => so.Lines)
                .OrderByDescending(so => so.OrderDate)
                .Select(so => new SalesOrderListItemVm
                {
                    Id = so.Id,
                    SoNumber = so.SoNumber,
                    CustomerName = so.Customer != null ? so.Customer.Name : string.Empty,
                    WarehouseName = so.Warehouse != null ? so.Warehouse.Name : string.Empty,
                    Status = so.Status,
                    OrderDate = so.OrderDate,
                    RequestedDate = so.RequestedDate,
                    TotalAmount = so.Lines.Sum(l => l.QuantityOrdered * l.UnitPrice)
                })
                .ToListAsync();

            return View(orders);
        }

        public async Task<IActionResult> Details(int id)
        {
            var so = await _unitOfWork.SalesOrders.GetWithLinesAsync(id);
            if (so is null)
                return NotFound();

            return View(ToDetailsVm(so));
        }

        [Authorize(Policy = "perm:inv.salesorders.create")]
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var vm = new SalesOrderFormVm();
            await PopulateSelectListsAsync(vm);
            return View(vm);
        }

        [Authorize(Policy = "perm:inv.salesorders.create")]
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SalesOrderFormVm model)
        {
            ValidateLines(model);

            if (!ModelState.IsValid)
            {
                await PopulateSelectListsAsync(model);
                return View(model);
            }

            var so = new SalesOrderModel
            {
                SoNumber = await GenerateUniqueSoNumberAsync(),
                CustomerId = model.CustomerId,
                WarehouseId = model.WarehouseId,
                RequestedDate = model.RequestedDate,
                Notes = string.IsNullOrWhiteSpace(model.Notes) ? null : model.Notes.Trim(),
                Status = SalesOrderStatus.Draft
            };

            foreach (var line in model.Lines.Where(l => l.ProductId > 0))
            {
                so.Lines.Add(new SalesOrderLineModel
                {
                    ProductId = line.ProductId,
                    QuantityOrdered = line.QuantityOrdered,
                    UnitPrice = line.UnitPrice
                });
            }

            await _unitOfWork.SalesOrders.AddAsync(so);
            await _unitOfWork.SaveChangesAsync();

            TempData["Success"] = $"Sales Order \"{so.SoNumber}\" created as Draft.";
            return RedirectToAction(nameof(Details), new { id = so.Id });
        }

        [Authorize(Policy = "perm:inv.salesorders.create")]
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(int id)
        {
            var so = await RequireStatusAsync(id, SalesOrderStatus.Draft, "submitted");
            if (so is null) return RedirectToAction(nameof(Details), new { id });

            so.Status = SalesOrderStatus.Submitted;
            so.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.SalesOrders.Update(so);
            await _unitOfWork.SaveChangesAsync();

            TempData["Success"] = $"Sales Order \"{so.SoNumber}\" submitted.";
            return RedirectToAction(nameof(Details), new { id });
        }

        [Authorize(Policy = "perm:inv.salesorders.create")]
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Fulfill(int id)
        {
            var so = await _unitOfWork.SalesOrders.GetWithLinesAsync(id);
            if (so is null)
                return NotFound();

            if (so.Status != SalesOrderStatus.Submitted)
            {
                TempData["Error"] = $"Only Submitted orders can be fulfilled (current status: {so.Status}).";
                return RedirectToAction(nameof(Details), new { id });
            }

            try
            {
                foreach (var line in so.Lines)
                {
                    var outstanding = line.QuantityOrdered - line.QuantityFulfilled;
                    if (outstanding <= 0)
                        continue;

                    await _stockService.IssueAsync(
                        line.ProductId, so.WarehouseId, outstanding,
                        StockMovementType.SalesIssue, "SalesOrder", so.Id,
                        $"Issued against SO {so.SoNumber}");

                    line.QuantityFulfilled = line.QuantityOrdered;
                }

                so.Status = SalesOrderStatus.Fulfilled;
                so.FulfilledAt = DateTime.UtcNow;
                so.UpdatedAt = DateTime.UtcNow;
                _unitOfWork.SalesOrders.Update(so);
                await _unitOfWork.SaveChangesAsync();

                TempData["Success"] = $"Sales Order \"{so.SoNumber}\" fulfilled - stock levels updated.";
            }
            catch (BadRequestException ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        [Authorize(Policy = "perm:inv.salesorders.create")]
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var so = await _unitOfWork.SalesOrders.GetByIdAsync(id);
            if (so is null)
                return NotFound();

            if (so.Status is not (SalesOrderStatus.Draft or SalesOrderStatus.Submitted))
            {
                TempData["Error"] = $"Only Draft or Submitted orders can be cancelled (current status: {so.Status}).";
                return RedirectToAction(nameof(Details), new { id });
            }

            so.Status = SalesOrderStatus.Cancelled;
            so.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.SalesOrders.Update(so);
            await _unitOfWork.SaveChangesAsync();

            TempData["Success"] = $"Sales Order \"{so.SoNumber}\" cancelled.";
            return RedirectToAction(nameof(Details), new { id });
        }


        private async Task<SalesOrderModel?> RequireStatusAsync(int id, SalesOrderStatus requiredStatus, string actionPastTense)
        {
            var so = await _unitOfWork.SalesOrders.GetByIdAsync(id);
            if (so is null)
                return null;

            if (so.Status != requiredStatus)
            {
                TempData["Error"] = $"Only {requiredStatus} orders can be {actionPastTense} (current status: {so.Status}).";
                return null;
            }

            return so;
        }

        private void ValidateLines(SalesOrderFormVm model)
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
                if (intendedLines[i].UnitPrice < 0)
                    ModelState.AddModelError(string.Empty, $"Line {i + 1}: unit price cannot be negative.");
            }
        }

        private async Task<string> GenerateUniqueSoNumberAsync()
        {
            var prefix = $"SO-{DateTime.UtcNow:yyyyMMdd}-";
            for (var attempt = 0; attempt < 5; attempt++)
            {
                var candidate = prefix + Random.Shared.Next(1000, 9999);
                if (!await _unitOfWork.SalesOrders.SoNumberExistsAsync(candidate))
                    return candidate;
            }

            return prefix + Guid.NewGuid().ToString("N")[..6];
        }

        private async Task PopulateSelectListsAsync(SalesOrderFormVm model)
        {
            model.Customers = await _unitOfWork.Customers.Query()
                .Where(c => c.IsActive)
                .OrderBy(c => c.Name)
                .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name })
                .ToListAsync();

            model.Warehouses = await _unitOfWork.Warehouses.Query()
                .Where(w => w.IsActive)
                .OrderBy(w => w.Name)
                .Select(w => new SelectListItem { Value = w.Id.ToString(), Text = w.Name })
                .ToListAsync();

            var products = await _unitOfWork.Products.Query()
                .Where(p => p.IsActive)
                .OrderBy(p => p.Name)
                .Select(p => new { p.Id, p.Name, p.SKU, p.SellingPrice })
                .ToListAsync();

            model.Products = products
                .Select(p => new SelectListItem { Value = p.Id.ToString(), Text = p.Name + " (" + p.SKU + ")" })
                .ToList();

            model.ProductPrices = products.ToDictionary(p => p.Id, p => p.SellingPrice);
        }

        private static SalesOrderDetailsVm ToDetailsVm(SalesOrderModel so) => new()
        {
            Id = so.Id,
            SoNumber = so.SoNumber,
            CustomerName = so.Customer?.Name ?? string.Empty,
            WarehouseName = so.Warehouse?.Name ?? string.Empty,
            Status = so.Status,
            OrderDate = so.OrderDate,
            RequestedDate = so.RequestedDate,
            FulfilledAt = so.FulfilledAt,
            Notes = so.Notes,
            Lines = so.Lines.Select(l => new SalesOrderLineDetailVm
            {
                ProductName = l.Product?.Name ?? string.Empty,
                SKU = l.Product?.SKU ?? string.Empty,
                Unit = l.Product?.Unit ?? string.Empty,
                QuantityOrdered = l.QuantityOrdered,
                QuantityFulfilled = l.QuantityFulfilled,
                UnitPrice = l.UnitPrice
            }).ToList()
        };
    }
}
