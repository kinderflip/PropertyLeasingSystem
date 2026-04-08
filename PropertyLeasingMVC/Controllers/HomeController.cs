using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PropertyLeasingAPI.Data;
using PropertyLeasingAPI.Models;
using PropertyLeasingMVC.ViewModels;

namespace PropertyLeasingMVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;

        public HomeController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var viewModel = new DashboardViewModel
            {
                TotalProperties = await _context.Properties.CountAsync(),
                AvailableProperties = await _context.Properties
                    .CountAsync(p => p.Status == PropertyStatus.Available),
                LeasedProperties = await _context.Properties
                    .CountAsync(p => p.Status == PropertyStatus.Leased),
                TotalTenants = await _context.Tenants.CountAsync(),
                ActiveLeases = await _context.Leases
                    .CountAsync(l => l.Status == LeaseStatus.Active),
                PendingMaintenance = await _context.MaintenanceRequests
                    .CountAsync(m => m.Status == MaintenanceStatus.Submitted || m.Status == MaintenanceStatus.Assigned),
                RecentRequests = await _context.MaintenanceRequests
                    .Include(m => m.Property)
                    .Include(m => m.Tenant)
                    .OrderByDescending(m => m.DateSubmitted)
                    .Take(5)
                    .ToListAsync(),
                RecentLeases = await _context.Leases
                    .Include(l => l.Property)
                    .Include(l => l.Tenant)
                    .OrderByDescending(l => l.StartDate)
                    .Take(5)
                    .ToListAsync()
            };

            return View(viewModel);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View();
        }
    }
}