using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SaaSInventoryManagement.Models;
using SaaSInventoryManagement.Repositories.Interfaces;
using SaaSInventoryManagement.ViewModels.Category;
using Microsoft.EntityFrameworkCore;

namespace SaaSInventoryManagement.Controllers
{
    [Authorize(Policy = "perm:inv.categories.view")]
    public class CategoryController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public CategoryController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IActionResult> Index()
        {
            var categories = await _unitOfWork.Categories.Query()
                .OrderBy(c => c.Name)
                .Select(c => new CategoryListItemVm
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    IsActive = c.IsActive,
                    CreatedAt = c.CreatedAt
                })
                .ToListAsync();

            return View(categories);
        }

        [Authorize(Policy = "perm:inv.categories.manage")]
        [HttpGet]
        public IActionResult Create() => View(new CategoryFormVm());

        [Authorize(Policy = "perm:inv.categories.manage")]
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CategoryFormVm model)
        {
            if (!ModelState.IsValid)
                return View(model);

            if (await _unitOfWork.Categories.NameExistsAsync(model.Name))
            {
                ModelState.AddModelError(nameof(model.Name), "A category with this name already exists.");
                return View(model);
            }

            var category = new Category
            {
                Name = model.Name.Trim(),
                Description = string.IsNullOrWhiteSpace(model.Description) ? null : model.Description.Trim(),
                IsActive = model.IsActive
            };

            await _unitOfWork.Categories.AddAsync(category);
            await _unitOfWork.SaveChangesAsync();

            TempData["Success"] = $"Category \"{category.Name}\" created.";
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Policy = "perm:inv.categories.manage")]
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var category = await _unitOfWork.Categories.GetByIdAsync(id);
            if (category is null)
                return NotFound();

            return View(new CategoryFormVm
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                IsActive = category.IsActive
            });
        }

        [Authorize(Policy = "perm:inv.categories.manage")]
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CategoryFormVm model)
        {
            if (id != model.Id)
                return BadRequest();

            if (!ModelState.IsValid)
                return View(model);

            var category = await _unitOfWork.Categories.GetByIdAsync(id);
            if (category is null)
                return NotFound();

            if (await _unitOfWork.Categories.NameExistsAsync(model.Name, excludingId: id))
            {
                ModelState.AddModelError(nameof(model.Name), "A category with this name already exists.");
                return View(model);
            }

            category.Name = model.Name.Trim();
            category.Description = string.IsNullOrWhiteSpace(model.Description) ? null : model.Description.Trim();
            category.IsActive = model.IsActive;
            category.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Categories.Update(category);
            await _unitOfWork.SaveChangesAsync();

            TempData["Success"] = $"Category \"{category.Name}\" updated.";
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Policy = "perm:inv.categories.manage")]
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _unitOfWork.Categories.GetByIdAsync(id);
            if (category is null)
                return NotFound();

            _unitOfWork.Categories.Remove(category);
            await _unitOfWork.SaveChangesAsync();

            TempData["Success"] = $"Category \"{category.Name}\" deleted.";
            return RedirectToAction(nameof(Index));
        }
    }
}
