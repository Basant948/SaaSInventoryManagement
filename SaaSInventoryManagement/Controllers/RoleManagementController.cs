using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SaaSInventoryManagement.Controllers
{
    [Authorize(Policy = "perm:inv.users.manage")]
    public class RoleManagementController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
