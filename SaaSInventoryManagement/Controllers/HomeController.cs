using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using SaaSInventoryManagement.Exceptions;
using SaaSInventoryManagement.Infrastructure.Authorization;
using SaaSInventoryManagement.Models;
using SaaSInventoryManagement.Services;
using SaaSInventoryManagement.ViewModels;
using System.Diagnostics;

namespace SaaSInventoryManagement.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly IMemoryCache _cache;
        private readonly NavigationService _navigation;

        public HomeController(IMemoryCache cache, NavigationService navigation)
        {
            _cache = cache;
            _navigation = navigation;
        }
        public async Task<IActionResult> Index()
        {
            var nav = await _navigation.GetNavigationAsync(User);
            return View(nav);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [Route("Home/Feature/{key}")]
        public async Task<IActionResult> Feature(string key)
        {
            var allowed = User.HasClaim(PermissionClaimTypes.Permission, "*")
                       || User.HasClaim(PermissionClaimTypes.Permission, key);

            if (!allowed)
                return Forbid();

            var nav = await _navigation.GetNavigationAsync(User);
            var item = nav.SelectMany(g => g.Items).FirstOrDefault(i => i.Key == key);

            if (item is null)
                return NotFound();

            return View(item);
        }

        [Route("Home/Error/{statusCode:int?}")]
        public IActionResult Error(int? statusCode, string? correlationId)
        {
            if (!string.IsNullOrEmpty(correlationId) &&
                _cache.TryGetValue($"error:{correlationId}", out ErrorViewModel? model) &&
                model is not null)
            {
                Response.StatusCode = model.StatusCode;
                return View(model);
            }

            var fallback = new ErrorViewModel
            {
                StatusCode = statusCode ?? 500,
                Message = statusCode == 404
                    ? "The page you're looking for could not be found."
                    : "An unexpected error occurred.",
                CorrelationId = correlationId
            };
            Response.StatusCode = fallback.StatusCode;
            return View(fallback);
        }
    }
}
