using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SaaSInventoryManagement.Models;
using SaaSInventoryManagement.Services.Interfaces_;
using SaaSInventoryManagement.ViewModels.Settings;
using Microsoft.EntityFrameworkCore;

namespace SaaSInventoryManagement.Controllers
{
    [Authorize(Policy = "perm:inv.settings.manage")]
    public class SettingsController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly ITenantProvider _tenantProvider;

        public SettingsController(ApplicationDbContext db, ITenantProvider tenantProvider)
        {
            _db = db;
            _tenantProvider = tenantProvider;
        }

        public async Task<IActionResult> Index()
        {
            var tenant = await GetOwnTenantAsync();
            if (tenant is null)
                return Forbid();

            return View(ToViewModel(tenant));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(CompanySettingsVm model)
        {
            var tenant = await GetOwnTenantAsync();
            if (tenant is null)
                return Forbid();

            if (!ModelState.IsValid)
            {
                model.Slug = tenant.Slug;
                model.IsActive = tenant.IsActive;
                model.CreatedAt = tenant.CreatedAt;
                return View(model);
            }

            tenant.Name = model.Name.Trim();
            tenant.ContactEmail = string.IsNullOrWhiteSpace(model.ContactEmail) ? null : model.ContactEmail.Trim();
            tenant.ContactPhone = string.IsNullOrWhiteSpace(model.ContactPhone) ? null : model.ContactPhone.Trim();
            tenant.Address = string.IsNullOrWhiteSpace(model.Address) ? null : model.Address.Trim();
            tenant.Currency = model.Currency.Trim().ToUpperInvariant();
            tenant.LowStockThreshold = model.LowStockThreshold;

            await _db.SaveChangesAsync();

            TempData["Success"] = "Company settings updated.";
            return RedirectToAction(nameof(Index));
        }

        private async Task<Tenant?> GetOwnTenantAsync()
        {
            if (_tenantProvider.TenantId is null)
                return null;

            return await _db.Tenants.FirstOrDefaultAsync(t => t.Id == _tenantProvider.TenantId);
        }

        private static CompanySettingsVm ToViewModel(Tenant tenant) => new()
        {
            Name = tenant.Name,
            Slug = tenant.Slug,
            ContactEmail = tenant.ContactEmail,
            ContactPhone = tenant.ContactPhone,
            Address = tenant.Address,
            Currency = tenant.Currency,
            LowStockThreshold = tenant.LowStockThreshold,
            IsActive = tenant.IsActive,
            CreatedAt = tenant.CreatedAt
        };
    }
}
