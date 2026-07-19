using Microsoft.AspNetCore.Mvc;
using SaaSInventoryManagement.ViewModels.Account;

namespace SaaSInventoryManagement.Controllers
{
    public class AccountController : Controller
    {
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Home");

            ViewData["ReturnUrl"] = returnUrl;
            return View(new LoginViewModel());
        }
    }
}
