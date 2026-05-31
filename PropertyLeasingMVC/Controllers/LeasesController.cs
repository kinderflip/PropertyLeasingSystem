using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using PropertyLeasingAPI.Data;
using PropertyLeasingAPI.Models;
using Microsoft.AspNetCore.Authorization;
using PropertyLeasingMVC.Hubs;
using PropertyLeasingMVC.ViewModels;

namespace PropertyLeasingMVC.Controllers
{
    [Authorize]
    public class LeasesController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IHubContext<NotificationHub> _notificationHub;

        public LeasesController(AppDbContext context, IHubContext<NotificationHub> notificationHub)
        {
            _context = context;
            _notificationHub = notificationHub;
        }

        // M5: tenant-scoped view of own leases. Surfaced in the navbar for Tenant role.
        [Authorize(Roles = "PropertyManager,Tenant")]
        public async Task<IActionResult> MyLeases(int page = 1)
        {
            ViewBag.MyView = true;
            var userId = User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier);
            var emptyPage = new List<Lease>();

            if (string.IsNullOrEmpty(userId)) return View("Index", emptyPage);

            var tenant = await _context.Tenants.FirstOrDefaultAsync(t => t.UserId == userId);
            if (tenant == null) return View("Index", emptyPage);

            var query = _context.Leases
                .Include(l => l.Property)
                .Include(l => l.Unit)
                .Include(l => l.Tenant)
                .Where(l => l.TenantId == tenant.TenantId)
                .OrderByDescending(l => l.ApplicationDate);

            ViewBag.MyView = true;
            return View("Index", await query.ToListAsync());
        }

        // GET: Leases — full list is a manager tool. Tenants use MyLeases instead.
        [Authorize(Roles = "PropertyManager")]
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

        // GET: Leases/Details/5 — a Tenant may only open their own lease.
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var lease = await _context.Leases
                .Include(l => l.Property)
                .Include(l => l.Unit)
                .Include(l => l.Tenant)
                .FirstOrDefaultAsync(m => m.LeaseId == id);

            if (lease == null) return NotFound();

            if (!await CanAccessTenantData(lease.TenantId)) return Forbid();

            return View(lease);
        }

        // Ownership gate: PropertyManager sees everything; anyone else may only
        // access rows belonging to their own linked tenant profile.
        private async Task<bool> CanAccessTenantData(int tenantId)
        {
            if (User.IsInRole("PropertyManager")) return true;
            var userId = User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return false;
            var tenant = await _context.Tenants.FirstOrDefaultAsync(t => t.UserId == userId);
            return tenant != null && tenant.TenantId == tenantId;
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
            // A Tenant may only apply on their own behalf — override any posted TenantId.
            if (User.IsInRole("Tenant") && !User.IsInRole("PropertyManager"))
            {
                var userId = User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier);
                var myTenant = await _context.Tenants.FirstOrDefaultAsync(t => t.UserId == userId);
                if (myTenant == null)
                {
                    ModelState.AddModelError(string.Empty, "Your account is not linked to a tenant profile. Contact the property manager.");
                }
                else
                {
                    lease.TenantId = myTenant.TenantId;
                    // The tenant form doesn't post TenantId, so clear its "required" model error.
                    ModelState.Remove(nameof(Lease.TenantId));
                }
            }

            // Auto-fill MonthlyRent from the chosen Unit/Property when missing.
            if (lease.MonthlyRent <= 0)
            {
                if (lease.UnitId.HasValue)
                {
                    var unit = await _context.Units
                        .FirstOrDefaultAsync(u => u.UnitId == lease.UnitId.Value);
                    if (unit != null) lease.MonthlyRent = unit.MonthlyRent;
                }
                else
                {
                    var prop = await _context.Properties
                        .FirstOrDefaultAsync(p => p.PropertyId == lease.PropertyId);
                    if (prop?.MonthlyRent != null) lease.MonthlyRent = prop.MonthlyRent.Value;
                }
            }

            if (lease.MonthlyRent <= 0)
                ModelState.AddModelError(nameof(Lease.MonthlyRent), "Monthly rent must be greater than 0.");

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
        public async Task<IActionResult> UpdateStatus(int id, LeaseStatus newStatus, string? screeningNotes, DateTime? newEndDate)
        {
            var lease = await _context.Leases
                .Include(l => l.Property)
                .Include(l => l.Unit)
                .FirstOrDefaultAsync(l => l.LeaseId == id);
            if (lease == null) return NotFound();

            // Allowed lease transitions
            bool allowed = false;
            if (lease.Status == LeaseStatus.Application && newStatus == LeaseStatus.Screening) allowed = true;
            else if (lease.Status == LeaseStatus.Screening && (newStatus == LeaseStatus.Approved || newStatus == LeaseStatus.Rejected)) allowed = true;
            else if (lease.Status == LeaseStatus.Approved && newStatus == LeaseStatus.Active) allowed = true;
            else if (lease.Status == LeaseStatus.Active && (newStatus == LeaseStatus.Renewal || newStatus == LeaseStatus.Terminated || newStatus == LeaseStatus.Expired)) allowed = true;
            else if (lease.Status == LeaseStatus.Renewal && (newStatus == LeaseStatus.Active || newStatus == LeaseStatus.Terminated)) allowed = true;

            if (!allowed)
            {
                TempData["ErrorMessage"] = $"Cannot transition from {lease.Status} to {newStatus}.";
                return RedirectToAction(nameof(Details), new { id });
            }

            // L14: Renewal → Active should extend the lease term. Accept an explicit new
            // EndDate from the renewal form; fall back to "+12 months" if the manager
            // didn't supply one. Reject dates that aren't strictly later than the current end.
            if (lease.Status == LeaseStatus.Renewal && newStatus == LeaseStatus.Active)
            {
                if (newEndDate.HasValue)
                {
                    if (newEndDate.Value.Date <= lease.EndDate.Date)
                    {
                        TempData["ErrorMessage"] = "Renewal end date must be later than the current end date.";
                        return RedirectToAction(nameof(Details), new { id });
                    }
                    lease.EndDate = newEndDate.Value;
                }
                else
                {
                    lease.EndDate = lease.EndDate.AddMonths(12);
                }
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

                // M2: auto-generate the monthly rent schedule for the lease term, but
                // only if no payments exist yet (avoids duplicates if Active is re-entered).
                var alreadyHasPayments = await _context.Payments.AnyAsync(p => p.LeaseId == lease.LeaseId);
                if (!alreadyHasPayments)
                {
                    var schedule = new List<Payment>();
                    for (var due = lease.StartDate.Date;
                         due <= lease.EndDate.Date;
                         due = due.AddMonths(1))
                    {
                        schedule.Add(new Payment
                        {
                            LeaseId     = lease.LeaseId,
                            Amount      = lease.MonthlyRent,
                            DueDate     = due,
                            PaymentType = PaymentType.Rent,
                            Status      = PaymentStatus.Pending
                        });
                    }
                    if (schedule.Count > 0) _context.Payments.AddRange(schedule);
                }
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
                    $"/Leases/Details/{lease.LeaseId}",
                    _notificationHub);
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
        public async Task<IActionResult> Edit(int id, [Bind("LeaseId,PropertyId,UnitId,TenantId,StartDate,EndDate,MonthlyRent,ApplicationDate,ApplicationNotes,ScreeningNotes,ApprovalDate")] Lease lease)
        {
            if (id != lease.LeaseId) return NotFound();

            // L2: Status is owned by the lifecycle (UpdateStatus + StatusTransitions).
            // Reload it from DB so a hand-crafted Edit POST cannot bypass the state machine.
            var existingStatus = await _context.Leases
                .Where(l => l.LeaseId == id)
                .Select(l => (LeaseStatus?)l.Status)
                .FirstOrDefaultAsync();
            if (existingStatus == null) return NotFound();
            lease.Status = existingStatus.Value;

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
                // If deleting an active lease, free the unit/property — but only when no
                // OTHER active lease still occupies it (avoids wrongly flipping to Available).
                if (lease.Status == LeaseStatus.Active)
                {
                    if (lease.Unit != null)
                    {
                        var otherActiveOnUnit = await _context.Leases.AnyAsync(l =>
                            l.LeaseId != lease.LeaseId && l.UnitId == lease.UnitId &&
                            l.Status == LeaseStatus.Active);
                        if (!otherActiveOnUnit)
                            lease.Unit.Status = UnitStatus.Available;
                    }
                    else if (lease.Property != null)
                    {
                        var otherActiveOnProperty = await _context.Leases.AnyAsync(l =>
                            l.LeaseId != lease.LeaseId && l.PropertyId == lease.PropertyId &&
                            l.UnitId == null && l.Status == LeaseStatus.Active);
                        if (!otherActiveOnProperty)
                            lease.Property.Status = PropertyStatus.Available;
                    }
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

            ViewBag.AllUnits = _context.Units
                .OrderBy(u => u.UnitNumber)
                .Select(u => new
                {
                    unitId = u.UnitId,
                    propertyId = u.PropertyId,
                    unitNumber = u.UnitNumber,
                    unitType = u.UnitType.ToString(),
                    monthlyRent = u.MonthlyRent,
                    status = u.Status.ToString()
                })
                .ToList();
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
