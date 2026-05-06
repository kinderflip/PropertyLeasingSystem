using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PropertyLeasingReports.Controllers
{
    // C7: the Reports portal landing page must be locked to the senior business role,
    // not just to "any authenticated user".
    [Authorize(Roles = Roles.PropertyManager)]
    public class HomeController : Controller
    {
        public IActionResult Index() => View();
    }
}