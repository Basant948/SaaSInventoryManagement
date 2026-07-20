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

        //  permission checklist for one user───────────────
        [HttpGet]
        public async Task<IActionResult> Permissions(string userId)
        {
            var targetUser = await GetTenantUserAsync(userId);
            if (targetUser is null)
                return NotFound();

            var roles = await _userManager.GetRolesAsync(targetUser);
            var isWildcard = roles.Contains("TenantAdmin") || roles.Contains("SuperAdmin");

            var grantedKeys = isWildcard
                ? new HashSet<string>()
                : (await _db.UserPermissions
                    .Where(up => up.UserId == userId)
                    .Select(up => up.Permission.Key)
                    .ToListAsync())
                    .ToHashSet();

            var catalog = await _db.Permissions
                .Where(p => p.IsActive)
                .OrderBy(p => p.SortOrder)
                .ToListAsync();

            var response = new UserPermissionsResponseVm
            {
                IsWildcard = isWildcard,
                Permissions = catalog.Select(p => new PermissionCheckItemVm
                {
                    Key = p.Key,
                    DisplayName = p.DisplayName,
                    GroupName = p.GroupName,
                    Granted = isWildcard || grantedKeys.Contains(p.Key)
                }).ToList()
            };

            return Json(response);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleSelection([FromBody] ToggleSelectionRequestVm request)
        {
            if (request is null || string.IsNullOrEmpty(request.UserId))
                return BadRequest(new { message = "A user must be specified." });

            var targetUser = await GetTenantUserAsync(request.UserId);
            if (targetUser is null)
                return NotFound();

            targetUser.IsPermissionManaged = request.Selected;
            await _db.SaveChangesAsync();

            return Json(new { success = true });
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
        private async Task<Applicationuser?> GetTenantUserAsync(string userId)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user is null)
                return null;

            if (!_tenantProvider.IsSuperAdmin && user.TenantId != _tenantProvider.TenantId)
                return null;

            return user;
        }
    }
}
