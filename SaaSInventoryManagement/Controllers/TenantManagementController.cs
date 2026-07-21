using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SaaSInventoryManagement.Models;
using SaaSInventoryManagement.Models.Identity;
using SaaSInventoryManagement.ViewModels.TenantManagement;

namespace SaaSInventoryManagement.Controllers
{
    [Authorize(Roles = "SuperAdmin")]
    public class TenantManagementController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<Applicationuser> _userManager;

        public TenantManagementController(ApplicationDbContext db, UserManager<Applicationuser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var tenants = await _db.Tenants
                .OrderBy(t => t.Name)
                .Select(t => new TenantListItemVm
                {
                    Id = t.Id,
                    Name = t.Name,
                    Slug = t.Slug,
                    IsActive = t.IsActive,
                    CreatedAt = t.CreatedAt,
                    UserCount = _db.Users.Count(u => u.TenantId == t.Id)
                })
                .ToListAsync();

            return View(tenants);
        }

        [HttpGet]
        public IActionResult Create() => View(new CreateTenantViewModel());

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateTenantViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var slug = model.Slug.Trim().ToLowerInvariant();

            if (await _db.Tenants.AnyAsync(t => t.Slug == slug))
            {
                ModelState.AddModelError(nameof(model.Slug), "That slug is already in use.");
                return View(model);
            }

            if (await _userManager.FindByEmailAsync(model.AdminEmail) != null)
            {
                ModelState.AddModelError(nameof(model.AdminEmail), "That email is already registered.");
                return View(model);
            }

            var tenant = new Tenant
            {
                Name = model.Name.Trim(),
                Slug = slug,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            _db.Tenants.Add(tenant);
            await _db.SaveChangesAsync();

            var admin = new Applicationuser
            {
                FirstName = model.AdminFirstName.Trim(),
                LastName = model.AdminLastName.Trim(),
                UserName = model.AdminEmail.Trim(),
                Email = model.AdminEmail.Trim(),
                TenantId = tenant.Id,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(admin, model.AdminPassword);
            if (!result.Succeeded)
            {
                _db.Tenants.Remove(tenant);
                await _db.SaveChangesAsync();

                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);

                return View(model);
            }

            await _userManager.AddToRoleAsync(admin, "TenantAdmin");

            TempData["Success"] = $"Tenant \"{tenant.Name}\" created with admin {admin.Email}.";
            return RedirectToAction(nameof(Index));
        }
    }
}
