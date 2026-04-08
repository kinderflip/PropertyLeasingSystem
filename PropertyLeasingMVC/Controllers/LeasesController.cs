using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PropertyLeasingAPI.Data;
using PropertyLeasingAPI.Models;
using Microsoft.AspNetCore.Authorization;

namespace PropertyLeasingMVC.Controllers
{
    [Authorize]
    public class LeasesController : Controller
    {
        private readonly AppDbContext _context;

        public LeasesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Leases
        public async Task<IActionResult> Index(string? searchString, LeaseStatus? status)
        {
            var leases = _context.Leases
                .Include(l => l.Property)
                .Include(l => l.Tenant)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
                leases = leases.Where(l =>
                    l.Property!.Address.Contains(searchString) ||
                    l.Tenant!.FullName.Contains(searchString));

            if (status.HasValue)
                leases = leases.Where(l => l.Status == status);

            ViewBag.SearchString = searchString;
            ViewBag.Status = status;

            return View(await leases.OrderByDescending(l => l.ApplicationDate).ToListAsync());
        }

        // GET: Leases/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var lease = await _context.Leases
                .Include(l => l.Property)
                .Include(l => l.Tenant)
                .FirstOrDefaultAsync(m => m.LeaseId == id);

            if (lease == null) return NotFound();

            return View(lease);
        }

        // GET: Leases/Apply - Tenant submits a lease application
        [Authorize(Roles = "PropertyManager,Tenant")]
        public IActionResult Create()
        {
            ViewData["PropertyId"] = new SelectList(
                _context.Properties.Where(p => p.Status == PropertyStatus.Available),
                "PropertyId", "Address");
            ViewData["TenantId"] = new SelectList(_context.Tenants, "TenantId", "FullName");
            return View();
        }

        // POST: Leases/Create - Submit lease application
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "PropertyManager,Tenant")]
        public async Task<IActionResult> Create([Bind("LeaseId,PropertyId,TenantId,StartDate,EndDate,MonthlyRent,ApplicationNotes")] Lease lease)
        {
            if (ModelState.IsValid)
            {
                // Check if property is already leased
                var existingActive = await _context.Leases
                    .AnyAsync(l => l.PropertyId == lease.PropertyId
                        && (l.Status == LeaseStatus.Active || l.Status == LeaseStatus.Approved));
                if (existingActive)
                {
                    ModelState.AddModelError("PropertyId", "This property already has an active or approved lease.");
                    ViewData["PropertyId"] = new SelectList(
                        _context.Properties.Where(p => p.Status == PropertyStatus.Available),
                        "PropertyId", "Address", lease.PropertyId);
                    ViewData["TenantId"] = new SelectList(_context.Tenants, "TenantId", "FullName", lease.TenantId);
                    return View(lease);
                }

                lease.Status = LeaseStatus.Application;
                lease.ApplicationDate = DateTime.Now;
                _context.Add(lease);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Lease application submitted successfully!";
                return RedirectToAction(nameof(Index));
            }
            ViewData["PropertyId"] = new SelectList(
                _context.Properties.Where(p => p.Status == PropertyStatus.Available),
                "PropertyId", "Address", lease.PropertyId);
            ViewData["TenantId"] = new SelectList(_context.Tenants, "TenantId", "FullName", lease.TenantId);
            return View(lease);
        }

        // POST: Leases/UpdateStatus/5 - Property Manager moves lease through lifecycle
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "PropertyManager")]
        public async Task<IActionResult> UpdateStatus(int id, LeaseStatus newStatus, string? screeningNotes)
        {
            var lease = await _context.Leases.Include(l => l.Property).FirstOrDefaultAsync(l => l.LeaseId == id);
            if (lease == null) return NotFound();

            // Validate status transitions
            bool validTransition = (lease.Status, newStatus) switch
            {
                (LeaseStatus.Application, LeaseStatus.Screening) => true,
                (LeaseStatus.Screening, LeaseStatus.Approved) => true,
                (LeaseStatus.Screening, LeaseStatus.Rejected) => true,
                (LeaseStatus.Approved, LeaseStatus.Active) => true,
                (LeaseStatus.Active, LeaseStatus.Renewal) => true,
                (LeaseStatus.Active, LeaseStatus.Terminated) => true,
                (LeaseStatus.Renewal, LeaseStatus.Active) => true,
                (LeaseStatus.Renewal, LeaseStatus.Terminated) => true,
                _ => false
            };

            if (!validTransition)
            {
                TempData["ErrorMessage"] = $"Cannot transition from {lease.Status} to {newStatus}.";
                return RedirectToAction(nameof(Details), new { id });
            }

            lease.Status = newStatus;

            if (!string.IsNullOrEmpty(screeningNotes))
                lease.ScreeningNotes = screeningNotes;

            if (newStatus == LeaseStatus.Approved)
                lease.ApprovalDate = DateTime.Now;

            // When lease becomes Active, mark property as Leased
            if (newStatus == LeaseStatus.Active && lease.Property != null)
                lease.Property.Status = PropertyStatus.Leased;

            // When lease is Terminated/Rejected, mark property as Available
            if ((newStatus == LeaseStatus.Terminated || newStatus == LeaseStatus.Rejected) && lease.Property != null)
                lease.Property.Status = PropertyStatus.Available;

            await _context.SaveChangesAsync();

            // Send notification to the tenant
            var tenant = await _context.Tenants.FirstOrDefaultAsync(t => t.TenantId == lease.TenantId);
            if (tenant?.UserId != null)
            {
                await NotificationsController.CreateNotification(_context,
                    tenant.UserId,
                    $"Lease {newStatus}",
                    $"Your lease application for {lease.Property?.Address} has been updated to {newStatus}.",
                    NotificationType.LeaseUpdate,
                    $"/Leases/Details/{lease.LeaseId}");
            }

            TempData["SuccessMessage"] = $"Lease status updated to {newStatus} successfully!";
            return RedirectToAction(nameof(Details), new { id });
        }

        // GET: Leases/Edit/5
        [Authorize(Roles = "PropertyManager")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var lease = await _context.Leases.FindAsync(id);
            if (lease == null) return NotFound();

            ViewData["PropertyId"] = new SelectList(_context.Properties, "PropertyId", "Address", lease.PropertyId);
            ViewData["TenantId"] = new SelectList(_context.Tenants, "TenantId", "FullName", lease.TenantId);
            return View(lease);
        }

        // POST: Leases/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "PropertyManager")]
        public async Task<IActionResult> Edit(int id, [Bind("LeaseId,PropertyId,TenantId,StartDate,EndDate,MonthlyRent,Status,ApplicationDate,ApplicationNotes,ScreeningNotes,ApprovalDate")] Lease lease)
        {
            if (id != lease.LeaseId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(lease);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Lease updated successfully!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!LeaseExists(lease.LeaseId))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["PropertyId"] = new SelectList(_context.Properties, "PropertyId", "Address", lease.PropertyId);
            ViewData["TenantId"] = new SelectList(_context.Tenants, "TenantId", "FullName", lease.TenantId);
            return View(lease);
        }

        // GET: Leases/Delete/5
        [Authorize(Roles = "PropertyManager")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var lease = await _context.Leases
                .Include(l => l.Property)
                .Include(l => l.Tenant)
                .FirstOrDefaultAsync(m => m.LeaseId == id);

            if (lease == null) return NotFound();

            return View(lease);
        }

        // POST: Leases/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "PropertyManager")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var lease = await _context.Leases.Include(l => l.Property).FirstOrDefaultAsync(l => l.LeaseId == id);
            if (lease != null)
            {
                // If deleting an active lease, make property available again
                if (lease.Status == LeaseStatus.Active && lease.Property != null)
                    lease.Property.Status = PropertyStatus.Available;

                _context.Leases.Remove(lease);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Lease deleted successfully!";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool LeaseExists(int id)
        {
            return _context.Leases.Any(e => e.LeaseId == id);
        }
    }
}
