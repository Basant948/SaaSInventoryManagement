using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SaaSInventoryManagement.Repositories.Interfaces;
using SaaSInventoryManagement.Services.Interfaces_;
using Microsoft.EntityFrameworkCore;
using ProductModel = SaaSInventoryManagement.Models.Product;
using SaaSInventoryManagement.ViewModels.Product;

namespace SaaSInventoryManagement.Controllers
{
    [Authorize(Policy = "perm:inv.products.view")]
    public class ProductController : Controller
    {
        private const string ImageFolder = "products";

        private readonly IUnitOfWork _unitOfWork;
        private readonly IFileStorageService _fileStorage;

        public ProductController(IUnitOfWork unitOfWork, IFileStorageService fileStorage)
        {
            _unitOfWork = unitOfWork;
            _fileStorage = fileStorage;
        }

        public async Task<IActionResult> Index()
        {
            var products = await _unitOfWork.Products.Query()
                .Include(p => p.Category)
                .OrderBy(p => p.Name)
                .Select(p => new ProductListItemVm
                {
                    Id = p.Id,
                    Name = p.Name,
                    SKU = p.SKU,
                    CategoryName = p.Category != null ? p.Category.Name : string.Empty,
                    Unit = p.Unit,
                    CostPrice = p.CostPrice,
                    SellingPrice = p.SellingPrice,
                    Image = p.Image,
                    IsActive = p.IsActive,
                    CreatedAt = p.CreatedAt
                })
                .ToListAsync();

            return View(products);
        }

        [Authorize(Policy = "perm:inv.products.manage")]
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var vm = new ProductFormVm { Categories = await GetCategorySelectListAsync() };
            return View(vm);
        }

        [Authorize(Policy = "perm:inv.products.manage")]
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductFormVm model)
        {
            await ValidateAsync(model, excludingId: null);

            if (!ModelState.IsValid)
            {
                model.Categories = await GetCategorySelectListAsync();
                return View(model);
            }

            var product = new ProductModel
            {
                Name = model.Name.Trim(),
                SKU = model.SKU.Trim(),
                CategoryId = model.CategoryId,
                Description = string.IsNullOrWhiteSpace(model.Description) ? null : model.Description.Trim(),
                Unit = model.Unit.Trim(),
                CostPrice = model.CostPrice,
                SellingPrice = model.SellingPrice,
                IsActive = model.IsActive
            };

            if (model.ImageFile is not null)
                product.Image = await _fileStorage.SaveFileAsync(model.ImageFile, ImageFolder);

            await _unitOfWork.Products.AddAsync(product);
            await _unitOfWork.SaveChangesAsync();

            TempData["Success"] = $"Product \"{product.Name}\" created.";
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Policy = "perm:inv.products.manage")]
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(id);
            if (product is null)
                return NotFound();

            var vm = new ProductFormVm
            {
                Id = product.Id,
                Name = product.Name,
                SKU = product.SKU,
                CategoryId = product.CategoryId,
                Description = product.Description,
                Unit = product.Unit,
                CostPrice = product.CostPrice,
                SellingPrice = product.SellingPrice,
                ExistingImage = product.Image,
                IsActive = product.IsActive,
                Categories = await GetCategorySelectListAsync()
            };

            return View(vm);
        }

        [Authorize(Policy = "perm:inv.products.manage")]
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ProductFormVm model)
        {
            if (id != model.Id)
                return BadRequest();

            await ValidateAsync(model, excludingId: id);

            if (!ModelState.IsValid)
            {
                model.Categories = await GetCategorySelectListAsync();
                return View(model);
            }

            var product = await _unitOfWork.Products.GetByIdAsync(id);
            if (product is null)
                return NotFound();

            product.Name = model.Name.Trim();
            product.SKU = model.SKU.Trim();
            product.CategoryId = model.CategoryId;
            product.Description = string.IsNullOrWhiteSpace(model.Description) ? null : model.Description.Trim();
            product.Unit = model.Unit.Trim();
            product.CostPrice = model.CostPrice;
            product.SellingPrice = model.SellingPrice;
            product.IsActive = model.IsActive;
            product.UpdatedAt = DateTime.UtcNow;

            if (model.ImageFile is not null)
            {
                var oldImage = product.Image;
                product.Image = await _fileStorage.SaveFileAsync(model.ImageFile, ImageFolder);
                _fileStorage.DeleteFile(oldImage);
            }

            _unitOfWork.Products.Update(product);
            await _unitOfWork.SaveChangesAsync();

            TempData["Success"] = $"Product \"{product.Name}\" updated.";
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Policy = "perm:inv.products.manage")]
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(id);
            if (product is null)
                return NotFound();

            _unitOfWork.Products.Remove(product);
            await _unitOfWork.SaveChangesAsync();

            TempData["Success"] = $"Product \"{product.Name}\" deleted.";
            return RedirectToAction(nameof(Index));
        }


        private async Task ValidateAsync(ProductFormVm model, int? excludingId)
        {
            if (!ModelState.IsValid)
                return;

            if (await _unitOfWork.Products.SkuExistsAsync(model.SKU, excludingId))
                ModelState.AddModelError(nameof(model.SKU), "A product with this SKU already exists.");

            if (!await _unitOfWork.Categories.AnyAsync(c => c.Id == model.CategoryId))
                ModelState.AddModelError(nameof(model.CategoryId), "Select a valid category.");
        }

        private async Task<List<SelectListItem>> GetCategorySelectListAsync()
        {
            return await _unitOfWork.Categories.Query()
                .Where(c => c.IsActive)
                .OrderBy(c => c.Name)
                .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name })
                .ToListAsync();
        }
    }
}
