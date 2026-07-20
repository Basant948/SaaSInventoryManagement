using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SaaSInventoryManagement.Models.Identity;
using SaaSInventoryManagement.Services.Interfaces_;
using SaaSInventoryManagement.ViewModels.RoleManagement;
using Microsoft.EntityFrameworkCore;

namespace SaaSInventoryManagement.Controllers
{
    [Authorize(Policy = "perm:inv.users.manage")]
    public class RoleManagementController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<Applicationuser> _userManager;
        private readonly ITenantProvider _tenantProvider;

        public RoleManagementController(ApplicationDbContext db, ITenantProvider tenantProvider, UserManager<Applicationuser> userManager)
        {
            _db = db;
            _tenantProvider = tenantProvider;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var users = await GetTenantUsersAsync();
            return View(users);
        }

        private async Task<List<RoleManagementUserVm>> GetTenantUsersAsync()
        {
            IQueryable<Applicationuser> query = _db.Users.Where(u => u.TenantId != null);

            if (!_tenantProvider.IsSuperAdmin)
                query = query.Where(u => u.TenantId == _tenantProvider.TenantId);

            var users = await query.OrderBy(u => u.FirstName).ThenBy(u => u.LastName).ToListAsync();

            var result = new List<RoleManagementUserVm>();
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                result.Add(new RoleManagementUserVm
                {
                    Id = user.Id,
                    FullName = string.IsNullOrWhiteSpace(user.FirstName) ? user.Email! : $"{user.FirstName} {user.LastName}".Trim(),
                    Email = user.Email!,
                    Role = roles.FirstOrDefault() ?? "User",
                    IsSelected = user.IsPermissionManaged
                });
            }
            return result;
        }
        }
}
