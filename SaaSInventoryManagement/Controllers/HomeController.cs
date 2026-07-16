using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using SaaSInventoryManagement.Exceptions;
using SaaSInventoryManagement.Models;
using SaaSInventoryManagement.ViewModels;
using System.Diagnostics;

namespace SaaSInventoryManagement.Controllers
{
    public class HomeController : Controller
    {
        private readonly IMemoryCache _cache;

        public HomeController(IMemoryCache cache)
        {
            _cache = cache;
        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
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
