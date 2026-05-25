using System.Security.Claims;
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
            var vm = new DashboardViewModel();

            if (User.IsInRole("PropertyManager"))
            {
                vm.Role = "PropertyManager";
                await LoadManagerDashboard(vm);
            }
            else if (User.IsInRole("MaintenanceStaff"))
            {
                vm.Role = "MaintenanceStaff";
                await LoadStaffDashboard(vm);
            }
            else if (User.IsInRole("Tenant"))
            {
                vm.Role = "Tenant";
                await LoadTenantDashboard(vm);
            }
            else
            {
                // Anonymous / guest view. Populate just the counts that help a prospective tenant.
                vm.Role = "Guest";
                vm.AvailableUnits = await _context.Units.CountAsync(u => u.Status == UnitStatus.Available);
                vm.AvailableProperties = await _context.Properties
                    .Include(p => p.Units)
                    .CountAsync(p => (p.Status == PropertyStatus.Available && !p.Units.Any())
                                   || p.Units.Any(u => u.Status == UnitStatus.Available));
            }

            return View(vm);
        }

        private async Task LoadManagerDashboard(DashboardViewModel vm)
        {
            vm.TotalProperties = await _context.Properties.CountAsync();
            vm.MultiUnitProperties = await _context.Properties.CountAsync(p => p.Units.Any());
            vm.StandaloneProperties = vm.TotalProperties - vm.MultiUnitProperties;
            vm.TotalUnits = await _context.Units.CountAsync();
            vm.AvailableUnits = await _context.Units.CountAsync(u => u.Status == UnitStatus.Available);

            // "Available" = standalone property marked Available OR multi-unit with any available unit
            vm.AvailableProperties = await _context.Properties
                .Include(p => p.Units)
                .CountAsync(p => (p.Status == PropertyStatus.Available && !p.Units.Any())
                               || p.Units.Any(u => u.Status == UnitStatus.Available));

            vm.LeasedProperties = await _context.Properties
                .CountAsync(p => p.Status == PropertyStatus.Leased && !p.Units.Any())
                + await _context.Units.CountAsync(u => u.Status == UnitStatus.Leased);

            vm.TotalTenants = await _context.Tenants.CountAsync();
            vm.ActiveLeases = await _context.Leases.CountAsync(l => l.Status == LeaseStatus.Active);
            vm.PendingMaintenance = await _context.MaintenanceRequests
                .CountAsync(m => m.Status == MaintenanceStatus.Submitted || m.Status == MaintenanceStatus.Assigned);

            vm.RecentRequests = await _context.MaintenanceRequests
                .Include(m => m.Property)
                .Include(m => m.Unit)
                .Include(m => m.Tenant)
                .OrderByDescending(m => m.DateSubmitted)
                .Take(5)
                .ToListAsync();

            vm.RecentLeases = await _context.Leases
                .Include(l => l.Property)
                .Include(l => l.Unit)
                .Include(l => l.Tenant)
                .OrderByDescending(l => l.StartDate)
                .Take(5)
                .ToListAsync();
        }

        private async Task LoadStaffDashboard(DashboardViewModel vm)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return;

            vm.AssignedToMe = await _context.MaintenanceRequests
                .CountAsync(m => m.AssignedStaffId == userId && m.Status == MaintenanceStatus.Assigned);
            vm.InProgressByMe = await _context.MaintenanceRequests
                .CountAsync(m => m.AssignedStaffId == userId && m.Status == MaintenanceStatus.InProgress);
            vm.CompletedByMe = await _context.MaintenanceRequests
                .CountAsync(m => m.AssignedStaffId == userId
                              && (m.Status == MaintenanceStatus.Resolved || m.Status == MaintenanceStatus.Closed));

            vm.RecentRequests = await _context.MaintenanceRequests
                .Include(m => m.Property)
                .Include(m => m.Unit)
                .Include(m => m.Tenant)
                .Where(m => m.AssignedStaffId == userId)
                .OrderByDescending(m => m.DateSubmitted)
                .Take(5)
                .ToListAsync();
        }

        private async Task LoadTenantDashboard(DashboardViewModel vm)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return;

            var tenant = await _context.Tenants.FirstOrDefaultAsync(t => t.UserId == userId);
            if (tenant == null) return;

            vm.MyActiveLeases = await _context.Leases
                .CountAsync(l => l.TenantId == tenant.TenantId && l.Status == LeaseStatus.Active);
            vm.MyOpenRequests = await _context.MaintenanceRequests
                .CountAsync(m => m.TenantId == tenant.TenantId
                              && m.Status != MaintenanceStatus.Resolved
                              && m.Status != MaintenanceStatus.Closed);
            vm.MyOverduePayments = await _context.Payments
                .CountAsync(p => p.Lease!.TenantId == tenant.TenantId && p.Status == PaymentStatus.Overdue);

            vm.RecentLeases = await _context.Leases
                .Include(l => l.Property)
                .Include(l => l.Unit)
                .Where(l => l.TenantId == tenant.TenantId)
                .OrderByDescending(l => l.StartDate)
                .Take(5)
                .ToListAsync();

            vm.RecentRequests = await _context.MaintenanceRequests
                .Include(m => m.Property)
                .Include(m => m.Unit)
                .Where(m => m.TenantId == tenant.TenantId)
                .OrderByDescending(m => m.DateSubmitted)
                .Take(5)
                .ToListAsync();
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
