using Microsoft.AspNetCore.Mvc;

namespace PropertyLeasingReports.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            if (!User.Identity!.IsAuthenticated)
                return RedirectToAction("Login", "Account");

            return View();
        }
    }
}