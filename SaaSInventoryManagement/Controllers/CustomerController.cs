using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SaaSInventoryManagement.Repositories.Interfaces;
using SaaSInventoryManagement.ViewModels.Customer;
using Microsoft.EntityFrameworkCore;
using CustomerModel = SaaSInventoryManagement.Models.Customer;

namespace SaaSInventoryManagement.Controllers
{
    [Authorize(Policy = "perm:inv.customers.view")]
    public class CustomerController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public CustomerController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IActionResult> Index()
        {
            var customers = await _unitOfWork.Customers.Query()
                .OrderBy(c => c.Name)
                .Select(c => new CustomerListItemVm
                {
                    Id = c.Id,
                    Name = c.Name,
                    ContactPerson = c.ContactPerson,
                    Email = c.Email,
                    Phone = c.Phone,
                    IsActive = c.IsActive,
                    CreatedAt = c.CreatedAt
                })
                .ToListAsync();

            return View(customers);
        }

        [Authorize(Policy = "perm:inv.customers.manage")]
        [HttpGet]
        public IActionResult Create() => View(new CustomerFormVm());

        [Authorize(Policy = "perm:inv.customers.manage")]
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CustomerFormVm model)
        {
            if (!ModelState.IsValid)
                return View(model);

            if (await _unitOfWork.Customers.NameExistsAsync(model.Name))
            {
                ModelState.AddModelError(nameof(model.Name), "A customer with this name already exists.");
                return View(model);
            }

            var customer = new CustomerModel
            {
                Name = model.Name.Trim(),
                ContactPerson = string.IsNullOrWhiteSpace(model.ContactPerson) ? null : model.ContactPerson.Trim(),
                Email = string.IsNullOrWhiteSpace(model.Email) ? null : model.Email.Trim(),
                Phone = string.IsNullOrWhiteSpace(model.Phone) ? null : model.Phone.Trim(),
                Address = string.IsNullOrWhiteSpace(model.Address) ? null : model.Address.Trim(),
                IsActive = model.IsActive
            };

            await _unitOfWork.Customers.AddAsync(customer);
            await _unitOfWork.SaveChangesAsync();

            TempData["Success"] = $"Customer \"{customer.Name}\" created.";
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Policy = "perm:inv.customers.manage")]
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var customer = await _unitOfWork.Customers.GetByIdAsync(id);
            if (customer is null)
                return NotFound();

            return View(new CustomerFormVm
            {
                Id = customer.Id,
                Name = customer.Name,
                ContactPerson = customer.ContactPerson,
                Email = customer.Email,
                Phone = customer.Phone,
                Address = customer.Address,
                IsActive = customer.IsActive
            });
        }

        [Authorize(Policy = "perm:inv.customers.manage")]
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CustomerFormVm model)
        {
            if (id != model.Id)
                return BadRequest();

            if (!ModelState.IsValid)
                return View(model);

            var customer = await _unitOfWork.Customers.GetByIdAsync(id);
            if (customer is null)
                return NotFound();

            if (await _unitOfWork.Customers.NameExistsAsync(model.Name, excludingId: id))
            {
                ModelState.AddModelError(nameof(model.Name), "A customer with this name already exists.");
                return View(model);
            }

            customer.Name = model.Name.Trim();
            customer.ContactPerson = string.IsNullOrWhiteSpace(model.ContactPerson) ? null : model.ContactPerson.Trim();
            customer.Email = string.IsNullOrWhiteSpace(model.Email) ? null : model.Email.Trim();
            customer.Phone = string.IsNullOrWhiteSpace(model.Phone) ? null : model.Phone.Trim();
            customer.Address = string.IsNullOrWhiteSpace(model.Address) ? null : model.Address.Trim();
            customer.IsActive = model.IsActive;
            customer.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Customers.Update(customer);
            await _unitOfWork.SaveChangesAsync();

            TempData["Success"] = $"Customer \"{customer.Name}\" updated.";
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Policy = "perm:inv.customers.manage")]
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var customer = await _unitOfWork.Customers.GetByIdAsync(id);
            if (customer is null)
                return NotFound();

            _unitOfWork.Customers.Remove(customer);
            await _unitOfWork.SaveChangesAsync();

            TempData["Success"] = $"Customer \"{customer.Name}\" deleted.";
            return RedirectToAction(nameof(Index));
        }
    }
}
