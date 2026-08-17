using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SaaSInventoryManagement.Repositories.Interfaces;
using SaaSInventoryManagement.ViewModels.Warehouse;
using Microsoft.EntityFrameworkCore;
using WarehouseModel = SaaSInventoryManagement.Models.Warehouse;

namespace SaaSInventoryManagement.Controllers
{
    [Authorize(Policy = "perm:inv.warehouses.view")]
    public class WarehouseController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public WarehouseController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IActionResult> Index()
        {
            var warehouses = await _unitOfWork.Warehouses.Query()
                .OrderBy(w => w.Name)
                .Select(w => new WarehouseListItemVm
                {
                    Id = w.Id,
                    Name = w.Name,
                    Code = w.Code,
                    Address = w.Address,
                    IsActive = w.IsActive,
                    CreatedAt = w.CreatedAt
                })
                .ToListAsync();

            return View(warehouses);
        }

        [Authorize(Policy = "perm:inv.warehouses.manage")]
        [HttpGet]
        public IActionResult Create() => View(new WarehouseFormVm());

        [Authorize(Policy = "perm:inv.warehouses.manage")]
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(WarehouseFormVm model)
        {
            if (!ModelState.IsValid)
                return View(model);

            if (await _unitOfWork.Warehouses.CodeExistsAsync(model.Code))
            {
                ModelState.AddModelError(nameof(model.Code), "A warehouse with this code already exists.");
                return View(model);
            }

            var warehouse = new WarehouseModel
            {
                Name = model.Name.Trim(),
                Code = model.Code.Trim().ToUpperInvariant(),
                Address = string.IsNullOrWhiteSpace(model.Address) ? null : model.Address.Trim(),
                IsActive = model.IsActive
            };

            await _unitOfWork.Warehouses.AddAsync(warehouse);
            await _unitOfWork.SaveChangesAsync();

            TempData["Success"] = $"Warehouse \"{warehouse.Name}\" created.";
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Policy = "perm:inv.warehouses.manage")]
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var warehouse = await _unitOfWork.Warehouses.GetByIdAsync(id);
            if (warehouse is null)
                return NotFound();

            return View(new WarehouseFormVm
            {
                Id = warehouse.Id,
                Name = warehouse.Name,
                Code = warehouse.Code,
                Address = warehouse.Address,
                IsActive = warehouse.IsActive
            });
        }

        [Authorize(Policy = "perm:inv.warehouses.manage")]
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, WarehouseFormVm model)
        {
            if (id != model.Id)
                return BadRequest();

            if (!ModelState.IsValid)
                return View(model);

            var warehouse = await _unitOfWork.Warehouses.GetByIdAsync(id);
            if (warehouse is null)
                return NotFound();

            if (await _unitOfWork.Warehouses.CodeExistsAsync(model.Code, excludingId: id))
            {
                ModelState.AddModelError(nameof(model.Code), "A warehouse with this code already exists.");
                return View(model);
            }

            warehouse.Name = model.Name.Trim();
            warehouse.Code = model.Code.Trim().ToUpperInvariant();
            warehouse.Address = string.IsNullOrWhiteSpace(model.Address) ? null : model.Address.Trim();
            warehouse.IsActive = model.IsActive;
            warehouse.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Warehouses.Update(warehouse);
            await _unitOfWork.SaveChangesAsync();

            TempData["Success"] = $"Warehouse \"{warehouse.Name}\" updated.";
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Policy = "perm:inv.warehouses.manage")]
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var warehouse = await _unitOfWork.Warehouses.GetByIdAsync(id);
            if (warehouse is null)
                return NotFound();

            _unitOfWork.Warehouses.Remove(warehouse);
            await _unitOfWork.SaveChangesAsync();

            TempData["Success"] = $"Warehouse \"{warehouse.Name}\" deleted.";
            return RedirectToAction(nameof(Index));
        }
    }
}
