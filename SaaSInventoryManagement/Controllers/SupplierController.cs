using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SaaSInventoryManagement.Repositories.Interfaces;
using SaaSInventoryManagement.ViewModels.Supplier;
using Microsoft.EntityFrameworkCore;
using SupplierModel = SaaSInventoryManagement.Models.Supplier;

namespace SaaSInventoryManagement.Controllers
{
    [Authorize(Policy = "perm:inv.suppliers.view")]
    public class SupplierController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public SupplierController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IActionResult> Index()
        {
            var suppliers = await _unitOfWork.Suppliers.Query()
                .OrderBy(s => s.Name)
                .Select(s => new SupplierListItemVm
                {
                    Id = s.Id,
                    Name = s.Name,
                    ContactPerson = s.ContactPerson,
                    Email = s.Email,
                    Phone = s.Phone,
                    IsActive = s.IsActive,
                    CreatedAt = s.CreatedAt
                })
                .ToListAsync();

            return View(suppliers);
        }

        [Authorize(Policy = "perm:inv.suppliers.manage")]
        [HttpGet]
        public IActionResult Create() => View(new SupplierFormVm());

        [Authorize(Policy = "perm:inv.suppliers.manage")]
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SupplierFormVm model)
        {
            if (!ModelState.IsValid)
                return View(model);

            if (await _unitOfWork.Suppliers.NameExistsAsync(model.Name))
            {
                ModelState.AddModelError(nameof(model.Name), "A supplier with this name already exists.");
                return View(model);
            }

            var supplier = new SupplierModel
            {
                Name = model.Name.Trim(),
                ContactPerson = string.IsNullOrWhiteSpace(model.ContactPerson) ? null : model.ContactPerson.Trim(),
                Email = string.IsNullOrWhiteSpace(model.Email) ? null : model.Email.Trim(),
                Phone = string.IsNullOrWhiteSpace(model.Phone) ? null : model.Phone.Trim(),
                Address = string.IsNullOrWhiteSpace(model.Address) ? null : model.Address.Trim(),
                IsActive = model.IsActive
            };

            await _unitOfWork.Suppliers.AddAsync(supplier);
            await _unitOfWork.SaveChangesAsync();

            TempData["Success"] = $"Supplier \"{supplier.Name}\" created.";
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Policy = "perm:inv.suppliers.manage")]
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var supplier = await _unitOfWork.Suppliers.GetByIdAsync(id);
            if (supplier is null)
                return NotFound();

            return View(new SupplierFormVm
            {
                Id = supplier.Id,
                Name = supplier.Name,
                ContactPerson = supplier.ContactPerson,
                Email = supplier.Email,
                Phone = supplier.Phone,
                Address = supplier.Address,
                IsActive = supplier.IsActive
            });
        }

        [Authorize(Policy = "perm:inv.suppliers.manage")]
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, SupplierFormVm model)
        {
            if (id != model.Id)
                return BadRequest();

            if (!ModelState.IsValid)
                return View(model);

            var supplier = await _unitOfWork.Suppliers.GetByIdAsync(id);
            if (supplier is null)
                return NotFound();

            if (await _unitOfWork.Suppliers.NameExistsAsync(model.Name, excludingId: id))
            {
                ModelState.AddModelError(nameof(model.Name), "A supplier with this name already exists.");
                return View(model);
            }

            supplier.Name = model.Name.Trim();
            supplier.ContactPerson = string.IsNullOrWhiteSpace(model.ContactPerson) ? null : model.ContactPerson.Trim();
            supplier.Email = string.IsNullOrWhiteSpace(model.Email) ? null : model.Email.Trim();
            supplier.Phone = string.IsNullOrWhiteSpace(model.Phone) ? null : model.Phone.Trim();
            supplier.Address = string.IsNullOrWhiteSpace(model.Address) ? null : model.Address.Trim();
            supplier.IsActive = model.IsActive;
            supplier.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Suppliers.Update(supplier);
            await _unitOfWork.SaveChangesAsync();

            TempData["Success"] = $"Supplier \"{supplier.Name}\" updated.";
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Policy = "perm:inv.suppliers.manage")]
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var supplier = await _unitOfWork.Suppliers.GetByIdAsync(id);
            if (supplier is null)
                return NotFound();

            _unitOfWork.Suppliers.Remove(supplier);
            await _unitOfWork.SaveChangesAsync();

            TempData["Success"] = $"Supplier \"{supplier.Name}\" deleted.";
            return RedirectToAction(nameof(Index));
        }
    }
}
