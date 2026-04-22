using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PropertyLeasingAPI.Data;
using PropertyLeasingAPI.Models;
using PropertyLeasingAPI.Services;
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
                .Include(l => l.Unit)
                .Include(l => l.Tenant)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
                leases = leases.Where(l =>
                    l.Property!.Address.Contains(searchString) ||
                    l.Tenant!.FullName.Contains(searchString) ||
                    (l.Unit != null && l.Unit.UnitNumber.Contains(searchString)));

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
                .Include(l => l.Unit)
                .Include(l => l.Tenant)
                .FirstOrDefaultAsync(m => m.LeaseId == id);

            if (lease == null) return NotFound();

            return View(lease);
        }

        // GET: Leases/Apply - Tenant submits a lease application
        [Authorize(Roles = "PropertyManager,Tenant")]
        public IActionResult Create()
        {
            PopulateDropdowns(null, null);
            return View();
        }

        // POST: Leases/Create - Submit lease application
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "PropertyManager,Tenant")]
        public async Task<IActionResult> Create([Bind("LeaseId,PropertyId,UnitId,TenantId,StartDate,EndDate,MonthlyRent,ApplicationNotes")] Lease lease)
        {
            // Validate unit vs standalone (multi-unit must have UnitId, standalone must not)
            var validationError = await ValidateUnitVsStandalone(lease);
            if (validationError != null)
                ModelState.AddModelError("UnitId", validationError);

            // Check for overlapping active/approved leases
            if (ModelState.IsValid && await HasOverlap(lease))
            {
                ModelState.AddModelError(string.Empty,
                    "This property/unit already has an active or approved lease for the selected dates.");
            }

            if (ModelState.IsValid)
            {
                // Default MonthlyRent if not provided
                if (lease.MonthlyRent <= 0)
                {
                    if (lease.UnitId.HasValue)
                    {
                        var unit = await _context.Units.FindAsync(lease.UnitId.Value);
                        if (unit != null) lease.MonthlyRent = unit.MonthlyRent;
                    }
                    else
                    {
                        var prop = await _context.Properties.FindAsync(lease.PropertyId);
                        if (prop?.MonthlyRent != null) lease.MonthlyRent = prop.MonthlyRent.Value;
                    }
                }

                lease.Status = LeaseStatus.Application;
                lease.ApplicationDate = DateTime.Now;
                _context.Add(lease);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Lease application submitted successfully!";
                return RedirectToAction(nameof(Index));
            }

            PopulateDropdowns(lease.PropertyId, lease.TenantId);
            return View(lease);
        }

        // POST: Leases/UpdateStatus/5 - Property Manager moves lease through lifecycle
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "PropertyManager")]
        public async Task<IActionResult> UpdateStatus(int id, LeaseStatus newStatus, string? screeningNotes)
        {
            var lease = await _context.Leases
                .Include(l => l.Property)
                .Include(l => l.Unit)
                .FirstOrDefaultAsync(l => l.LeaseId == id);
            if (lease == null) return NotFound();

            // Shared rule set — same as API
            if (!StatusTransitions.IsValidLeaseTransition(lease.Status, newStatus))
            {
                TempData["ErrorMessage"] = $"Cannot transition from {lease.Status} to {newStatus}.";
                return RedirectToAction(nameof(Details), new { id });
            }

            lease.Status = newStatus;

            if (!string.IsNullOrEmpty(screeningNotes))
                lease.ScreeningNotes = screeningNotes;

            if (newStatus == LeaseStatus.Approved)
                lease.ApprovalDate = DateTime.Now;

            // Status propagation: keep Unit.Status / Property.Status in sync
            if (newStatus == LeaseStatus.Active)
            {
                if (lease.Unit != null)
                    lease.Unit.Status = UnitStatus.Leased;
                else if (lease.Property != null)
                    lease.Property.Status = PropertyStatus.Leased;
            }
            else if (newStatus == LeaseStatus.Terminated || newStatus == LeaseStatus.Rejected || newStatus == LeaseStatus.Expired)
            {
                if (lease.Unit != null)
                    lease.Unit.Status = UnitStatus.Available;
                else if (lease.Property != null)
                    lease.Property.Status = PropertyStatus.Available;
            }

            await _context.SaveChangesAsync();

            // Send notification to the tenant
            var tenant = await _context.Tenants.FirstOrDefaultAsync(t => t.TenantId == lease.TenantId);
            if (tenant?.UserId != null)
            {
                var location = lease.Property?.Address + (lease.Unit != null ? $" (Unit {lease.Unit.UnitNumber})" : "");
                await NotificationsController.CreateNotification(_context,
                    tenant.UserId,
                    $"Lease {newStatus}",
                    $"Your lease application for {location} has been updated to {newStatus}.",
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

            PopulateDropdowns(lease.PropertyId, lease.TenantId);
            return View(lease);
        }

        // POST: Leases/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "PropertyManager")]
        public async Task<IActionResult> Edit(int id, [Bind("LeaseId,PropertyId,UnitId,TenantId,StartDate,EndDate,MonthlyRent,Status,ApplicationDate,ApplicationNotes,ScreeningNotes,ApprovalDate")] Lease lease)
        {
            if (id != lease.LeaseId) return NotFound();

            var validationError = await ValidateUnitVsStandalone(lease);
            if (validationError != null)
                ModelState.AddModelError("UnitId", validationError);

            if (ModelState.IsValid && await HasOverlap(lease))
            {
                ModelState.AddModelError(string.Empty,
                    "This property/unit already has an overlapping active or approved lease.");
            }

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
            PopulateDropdowns(lease.PropertyId, lease.TenantId);
            return View(lease);
        }

        // GET: Leases/Delete/5
        [Authorize(Roles = "PropertyManager")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var lease = await _context.Leases
                .Include(l => l.Property)
                .Include(l => l.Unit)
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
            var lease = await _context.Leases
                .Include(l => l.Property)
                .Include(l => l.Unit)
                .FirstOrDefaultAsync(l => l.LeaseId == id);
            if (lease != null)
            {
                // If deleting an active lease, make the unit/property available again
                if (lease.Status == LeaseStatus.Active)
                {
                    if (lease.Unit != null)
                        lease.Unit.Status = UnitStatus.Available;
                    else if (lease.Property != null)
                        lease.Property.Status = PropertyStatus.Available;
                }

                _context.Leases.Remove(lease);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Lease deleted successfully!";
            }

            return RedirectToAction(nameof(Index));
        }

        // --- helpers ---

        private void PopulateDropdowns(int? selectedProperty, int? selectedTenant)
        {
            // Show properties that are either standalone-available OR have any available unit
            var availableProperties = _context.Properties
                .Include(p => p.Units)
                .Where(p => (p.Status == PropertyStatus.Available && !p.Units.Any())
                         || p.Units.Any(u => u.Status == UnitStatus.Available))
                .Select(p => new { p.PropertyId, p.Address })
                .ToList();

            ViewData["PropertyId"] = new SelectList(availableProperties, "PropertyId", "Address", selectedProperty);
            ViewData["TenantId"] = new SelectList(_context.Tenants, "TenantId", "FullName", selectedTenant);
        }

        // Multi-unit => must set UnitId. Standalone => must leave UnitId null.
        private async Task<string?> ValidateUnitVsStandalone(Lease lease)
        {
            var property = await _context.Properties
                .Include(p => p.Units)
                .FirstOrDefaultAsync(p => p.PropertyId == lease.PropertyId);
            if (property == null) return "Selected property does not exist.";

            bool hasUnits = property.Units != null && property.Units.Any();

            if (hasUnits && !lease.UnitId.HasValue)
                return "This is a multi-unit property — you must select a unit.";

            if (!hasUnits && lease.UnitId.HasValue)
                return "This is a standalone property — do not select a unit.";

            if (lease.UnitId.HasValue)
            {
                var unitBelongs = property.Units!.Any(u => u.UnitId == lease.UnitId.Value);
                if (!unitBelongs) return "Selected unit does not belong to the selected property.";
            }

            return null;
        }

        // Multi-unit-aware overlap check (Plan L1/B6)
        private async Task<bool> HasOverlap(Lease lease)
        {
            return await _context.Leases.AnyAsync(l =>
                l.LeaseId != lease.LeaseId &&
                (l.Status == LeaseStatus.Active || l.Status == LeaseStatus.Approved) &&
                l.StartDate <= lease.EndDate &&
                l.EndDate >= lease.StartDate &&
                (
                    // Multi-unit: clash on same unit
                    (lease.UnitId != null && l.UnitId == lease.UnitId)
                    ||
                    // Standalone: clash on the whole property and neither side has a unit
                    (lease.UnitId == null && l.UnitId == null && l.PropertyId == lease.PropertyId)
                )
            );
        }

        private bool LeaseExists(int id)
        {
            return _context.Leases.Any(e => e.LeaseId == id);
        }
    }
}
