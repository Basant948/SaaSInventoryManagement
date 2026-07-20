using Microsoft.Extensions.Caching.Memory;
using SaaSInventoryManagement.Infrastructure.Authorization;
using SaaSInventoryManagement.ViewModels.Navigation;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace SaaSInventoryManagement.Services
{
    public class NavigationService
    {
        private const string CatalogCacheKey = "permission-catalog";
        private static readonly TimeSpan CatalogCacheDuration = TimeSpan.FromMinutes(30);

        private readonly ApplicationDbContext _db;
        private readonly IMemoryCache _cache;

        public NavigationService(ApplicationDbContext db, IMemoryCache cache)
        {
            _db = db;
            _cache = cache;
        }

        public async Task<List<NavGroupDto>> GetNavigationAsync(ClaimsPrincipal user)
        {
            var catalog = await GetCatalogAsync();

            var permClaims = user.FindAll(PermissionClaimTypes.Permission).Select(c => c.Value).ToHashSet();
            var isWildcard = permClaims.Contains("*");

            var visible = catalog.Where(p => isWildcard || permClaims.Contains(p.Key));

            return visible
                .GroupBy(p => p.GroupName)
                .OrderBy(g => g.Min(p => p.SortOrder))
                .Select(g => new NavGroupDto
                {
                    GroupName = g.Key,
                    Items = g.OrderBy(p => p.SortOrder).ToList()
                })
                .ToList();
        }

        private async Task<List<NavItemDto>> GetCatalogAsync()
        {
            if (_cache.TryGetValue(CatalogCacheKey, out List<NavItemDto>? cached) && cached is not null)
                return cached;

            var items = await _db.Permissions
                .Where(p => p.IsActive)
                .OrderBy(p => p.SortOrder)
                .Select(p => new NavItemDto
                {
                    Key = p.Key,
                    DisplayName = p.DisplayName,
                    GroupName = p.GroupName,
                    IconClass = p.IconClass,
                    ControllerAction = p.ControllerAction,
                    SortOrder = p.SortOrder
                })
                .ToListAsync();

            _cache.Set(CatalogCacheKey, items, CatalogCacheDuration);
            return items;
        }
    }
}
