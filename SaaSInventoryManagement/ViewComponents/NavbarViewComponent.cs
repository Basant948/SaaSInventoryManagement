using Microsoft.AspNetCore.Mvc;
using SaaSInventoryManagement.Services;
using SaaSInventoryManagement.ViewModels.Navigation;

namespace SaaSInventoryManagement.ViewComponents
{
    public class NavbarViewComponent : ViewComponent
    {
        private readonly NavigationService _navigation;

        public NavbarViewComponent(NavigationService navigation)
        {
            _navigation = navigation;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            if (UserClaims?.Identity?.IsAuthenticated != true)
                return View(new List<NavGroupDto>());

            var nav = await _navigation.GetNavigationAsync(UserClaims);
            return View(nav);
        }

        private System.Security.Claims.ClaimsPrincipal? UserClaims => HttpContext?.User;
    }
}
