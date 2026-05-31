using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using PropertyLeasingAPI.Data;
using PropertyLeasingAPI.Models;
using PropertyLeasingMVC.Hubs;
using PropertyLeasingMVC.ViewModels;

namespace PropertyLeasingMVC.Controllers
{
    [Authorize]
    public class MaintenanceRequestsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IHubContext<MaintenanceHub> _hubContext;
        private readonly IHubContext<NotificationHub> _notificationHub;
        private readonly UserManager<ApplicationUser> _userManager;

        public MaintenanceRequestsController(
            AppDbContext context,
            IHubContext<MaintenanceHub> hubContext,
            IHubContext<NotificationHub> notificationHub,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _hubContext = hubContext;
            _notificationHub = notificationHub;
            _userManager = userManager;
        }

        // GET: MaintenanceRequests
        public async Task<IActionResult> Index(string? searchString, MaintenanceStatus? status, MaintenanceCategory? category, MaintenancePriority? priority)
        {
            var requests = _context.MaintenanceRequests
                .Include(m => m.Property)
                .Include(m => m.Unit)
                .Include(m => m.Tenant)
                .Include(m => m.AssignedStaff)
                .AsQueryable();

            // A Tenant's "My Requests" must only show their own tickets, not everyone's.
            // Managers and maintenance staff keep the full board.
            if (User.IsInRole("Tenant") && !User.IsInRole("PropertyManager") && !User.IsInRole("MaintenanceStaff"))
            {
                var userId = _userManager.GetUserId(User);
                var myTenant = await _context.Tenants.FirstOrDefaultAsync(t => t.UserId == userId);
                var myTenantId = myTenant?.TenantId ?? -1;
                requests = requests.Where(m => m.TenantId == myTenantId);
            }

            if (!string.IsNullOrEmpty(searchString))
                requests = requests.Where(m =>
                    m.Title.Contains(searchString) ||
                    m.Description.Contains(searchString) ||
                    (m.Unit != null && m.Unit.UnitNumber.Contains(searchString)));

            if (status.HasValue)
                requests = requests.Where(m => m.Status == status);

            if (category.HasValue)
                requests = requests.Where(m => m.Category == category);

            if (priority.HasValue)
                requests = requests.Where(m => m.Priority == priority);

            ViewBag.SearchString = searchString;
            ViewBag.Status = status;
            ViewBag.Category = category;
            ViewBag.Priority = priority;

            return View(await requests.OrderByDescending(m => m.DateSubmitted).ToListAsync());
        }

        // GET: MaintenanceRequests/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var maintenanceRequest = await _context.MaintenanceRequests
                .Include(m => m.Property)
                .Include(m => m.Unit)
                .Include(m => m.Tenant)
                .Include(m => m.AssignedStaff)
                .FirstOrDefaultAsync(m => m.RequestId == id);

            if (maintenanceRequest == null) return NotFound();

            var staff = await _userManager.GetUsersInRoleAsync("MaintenanceStaff");
            ViewBag.StaffList = new SelectList(staff, "Id", "FullName", maintenanceRequest.AssignedStaffId);

            return View(maintenanceRequest);
        }

        // POST: MaintenanceRequests/Assign/5 - Property Manager assigns staff
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "PropertyManager")]
        public async Task<IActionResult> Assign(int id, string assignedStaffId)
        {
            var request = await _context.MaintenanceRequests
                .Include(m => m.Unit)
                .FirstOrDefaultAsync(m => m.RequestId == id);
            if (request == null) return NotFound();

            if (string.IsNullOrEmpty(assignedStaffId))
            {
                TempData["ErrorMessage"] = "Please select a staff member to assign.";
                return RedirectToAction(nameof(Details), new { id });
            }

            request.AssignedStaffId = assignedStaffId;
            request.DateAssigned = DateTime.Now;
            if (request.Status == MaintenanceStatus.Submitted)
                request.Status = MaintenanceStatus.Assigned;

            await _context.SaveChangesAsync();

            // Notify assigned staff
            await NotificationsController.CreateNotification(_context,
                assignedStaffId,
                "New Assignment",
                $"You have been assigned to maintenance request: {request.Title}",
                NotificationType.MaintenanceUpdate,
                $"/MaintenanceRequests/Details/{request.RequestId}",
                _notificationHub);

            // Notify tenant
            var tenant = await _context.Tenants.FirstOrDefaultAsync(t => t.TenantId == request.TenantId);
            if (tenant?.UserId != null)
            {
                await NotificationsController.CreateNotification(_context,
                    tenant.UserId,
                    "Request Assigned",
                    $"Your maintenance request \"{request.Title}\" has been assigned to staff.",
                    NotificationType.MaintenanceUpdate,
                    $"/MaintenanceRequests/Details/{request.RequestId}",
                    _notificationHub);
            }

            // Notify via SignalR — send group-scoped + include unit number
            await BroadcastStatusUpdate(request);

            TempData["SuccessMessage"] = "Staff assigned successfully!";
            return RedirectToAction(nameof(Details), new { id });
        }

        // POST: MaintenanceRequests/UpdateStatus/5 - Update status through lifecycle
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "PropertyManager,MaintenanceStaff")]
        public async Task<IActionResult> UpdateStatus(int id, MaintenanceStatus newStatus, string? staffNotes)
        {
            var request = await _context.MaintenanceRequests
                .Include(m => m.Unit)
                .FirstOrDefaultAsync(m => m.RequestId == id);
            if (request == null) return NotFound();

            // Allowed maintenance transitions
            bool allowed = false;
            if (request.Status == MaintenanceStatus.Submitted && (newStatus == MaintenanceStatus.Assigned || newStatus == MaintenanceStatus.InProgress)) allowed = true;
            else if (request.Status == MaintenanceStatus.Assigned && newStatus == MaintenanceStatus.InProgress) allowed = true;
            else if (request.Status == MaintenanceStatus.InProgress && (newStatus == MaintenanceStatus.Resolved || newStatus == MaintenanceStatus.Closed)) allowed = true;
            else if (request.Status == MaintenanceStatus.Resolved && newStatus == MaintenanceStatus.Closed) allowed = true;

            if (!allowed)
            {
                TempData["ErrorMessage"] = $"Cannot transition from {request.Status} to {newStatus}.";
                return RedirectToAction(nameof(Details), new { id });
            }

            request.Status = newStatus;

            if (!string.IsNullOrEmpty(staffNotes))
                request.StaffNotes = staffNotes;

            if (newStatus == MaintenanceStatus.Assigned && request.DateAssigned == null)
                request.DateAssigned = DateTime.Now;

            if ((newStatus == MaintenanceStatus.Resolved || newStatus == MaintenanceStatus.Closed)
                && request.DateResolved == null)
                request.DateResolved = DateTime.Now;

            await _context.SaveChangesAsync();

            // Notify tenant about status change
            var tenantForNotify = await _context.Tenants.FirstOrDefaultAsync(t => t.TenantId == request.TenantId);
            if (tenantForNotify?.UserId != null)
            {
                await NotificationsController.CreateNotification(_context,
                    tenantForNotify.UserId,
                    $"Request {newStatus}",
                    $"Your maintenance request \"{request.Title}\" status changed to {newStatus}.",
                    NotificationType.MaintenanceUpdate,
                    $"/MaintenanceRequests/Details/{request.RequestId}",
                    _notificationHub);
            }

            // If assigned staff exists, notify them too
            if (!string.IsNullOrEmpty(request.AssignedStaffId))
            {
                await NotificationsController.CreateNotification(_context,
                    request.AssignedStaffId,
                    $"Request {newStatus}",
                    $"Maintenance request \"{request.Title}\" status changed to {newStatus}.",
                    NotificationType.MaintenanceUpdate,
                    $"/MaintenanceRequests/Details/{request.RequestId}",
                    _notificationHub);
            }

            // Notify via SignalR — include unit number for the live board
            await BroadcastStatusUpdate(request);

            TempData["SuccessMessage"] = $"Status updated to {newStatus} successfully!";
            return RedirectToAction(nameof(Details), new { id });
        }

        // GET: MaintenanceRequests/Create
        [Authorize(Roles = "PropertyManager,Tenant")]
        public IActionResult Create()
        {
            ViewData["PropertyId"] = new SelectList(_context.Properties, "PropertyId", "Address");
            ViewData["TenantId"] = new SelectList(_context.Tenants, "TenantId", "FullName");
            PopulateAllUnits();
            return View();
        }

        // POST: MaintenanceRequests/Create
        [Authorize(Roles = "PropertyManager,Tenant")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("RequestId,PropertyId,UnitId,TenantId,Title,Description,Category,Priority")] MaintenanceRequest maintenanceRequest)
        {
            // A Tenant may only submit requests under their own name — override any posted TenantId.
            if (User.IsInRole("Tenant") && !User.IsInRole("PropertyManager"))
            {
                var userId = _userManager.GetUserId(User);
                var myTenant = await _context.Tenants.FirstOrDefaultAsync(t => t.UserId == userId);
                if (myTenant == null)
                {
                    ModelState.AddModelError(string.Empty, "Your account is not linked to a tenant profile. Contact the property manager.");
                }
                else
                {
                    maintenanceRequest.TenantId = myTenant.TenantId;
                    // The tenant form doesn't post TenantId, so clear its "required" model error.
                    ModelState.Remove(nameof(MaintenanceRequest.TenantId));
                }
            }

            var validationError = await ValidateUnitVsStandalone(maintenanceRequest);
            if (validationError != null)
                ModelState.AddModelError("UnitId", validationError);

            if (ModelState.IsValid)
            {
                maintenanceRequest.DateSubmitted = DateTime.Now;
                maintenanceRequest.Status = MaintenanceStatus.Submitted;
                _context.Add(maintenanceRequest);
                await _context.SaveChangesAsync();

                // Reload with Unit for the broadcast payload
                var full = await _context.MaintenanceRequests
                    .Include(m => m.Unit)
                    .FirstOrDefaultAsync(m => m.RequestId == maintenanceRequest.RequestId);
                if (full != null) await BroadcastStatusUpdate(full);

                TempData["SuccessMessage"] = "Maintenance request submitted successfully!";
                return RedirectToAction(nameof(Index));
            }
            ViewData["PropertyId"] = new SelectList(_context.Properties, "PropertyId", "Address", maintenanceRequest.PropertyId);
            ViewData["TenantId"] = new SelectList(_context.Tenants, "TenantId", "FullName", maintenanceRequest.TenantId);
            PopulateAllUnits();
            return View(maintenanceRequest);
        }

        // GET: MaintenanceRequests/Edit/5
        [Authorize(Roles = "PropertyManager,MaintenanceStaff")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var maintenanceRequest = await _context.MaintenanceRequests.FindAsync(id);
            if (maintenanceRequest == null) return NotFound();

            ViewData["PropertyId"] = new SelectList(_context.Properties, "PropertyId", "Address", maintenanceRequest.PropertyId);
            ViewData["TenantId"] = new SelectList(_context.Tenants, "TenantId", "FullName", maintenanceRequest.TenantId);

            var staffUsers = await _userManager.GetUsersInRoleAsync("MaintenanceStaff");
            ViewData["AssignedStaffId"] = new SelectList(staffUsers, "Id", "FullName", maintenanceRequest.AssignedStaffId);

            PopulateAllUnits();
            return View(maintenanceRequest);
        }

        // POST: MaintenanceRequests/Edit/5
        [Authorize(Roles = "PropertyManager,MaintenanceStaff")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("RequestId,PropertyId,UnitId,TenantId,Title,Description,Category,Priority,Status,AssignedStaffId,StaffNotes,DateSubmitted,DateAssigned,DateResolved")] MaintenanceRequest maintenanceRequest)
        {
            if (id != maintenanceRequest.RequestId) return NotFound();

            // L3: MaintenanceStaff are allowed to edit only Status + StaffNotes.
            // Reload everything else from DB so an over-posted form can't reassign tickets.
            if (User.IsInRole("MaintenanceStaff") && !User.IsInRole("PropertyManager"))
            {
                var existing = await _context.MaintenanceRequests
                    .FirstOrDefaultAsync(m => m.RequestId == id);
                if (existing == null) return NotFound();

                maintenanceRequest.PropertyId       = existing.PropertyId;
                maintenanceRequest.UnitId           = existing.UnitId;
                maintenanceRequest.TenantId         = existing.TenantId;
                maintenanceRequest.Title            = existing.Title;
                maintenanceRequest.Description      = existing.Description;
                maintenanceRequest.Category         = existing.Category;
                maintenanceRequest.Priority         = existing.Priority;
                maintenanceRequest.AssignedStaffId  = existing.AssignedStaffId;
                maintenanceRequest.DateSubmitted    = existing.DateSubmitted;
                maintenanceRequest.DateAssigned     = existing.DateAssigned;
                // Status + StaffNotes + DateResolved keep the values posted by the staff form.
            }

            var validationError = await ValidateUnitVsStandalone(maintenanceRequest);
            if (validationError != null)
                ModelState.AddModelError("UnitId", validationError);

            if (ModelState.IsValid)
            {
                try
                {
                    // Auto-set dates based on status
                    if (maintenanceRequest.Status == MaintenanceStatus.Assigned && maintenanceRequest.DateAssigned == null)
                        maintenanceRequest.DateAssigned = DateTime.Now;

                    if ((maintenanceRequest.Status == MaintenanceStatus.Resolved || maintenanceRequest.Status == MaintenanceStatus.Closed)
                        && maintenanceRequest.DateResolved == null)
                        maintenanceRequest.DateResolved = DateTime.Now;

                    _context.Update(maintenanceRequest);
                    await _context.SaveChangesAsync();

                    // Reload with Unit for the broadcast payload
                    var full = await _context.MaintenanceRequests
                        .Include(m => m.Unit)
                        .FirstOrDefaultAsync(m => m.RequestId == maintenanceRequest.RequestId);
                    if (full != null) await BroadcastStatusUpdate(full);

                    TempData["SuccessMessage"] = "Maintenance request updated successfully!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MaintenanceRequestExists(maintenanceRequest.RequestId))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["PropertyId"] = new SelectList(_context.Properties, "PropertyId", "Address", maintenanceRequest.PropertyId);
            ViewData["TenantId"] = new SelectList(_context.Tenants, "TenantId", "FullName", maintenanceRequest.TenantId);

            var staffList = await _userManager.GetUsersInRoleAsync("MaintenanceStaff");
            ViewData["AssignedStaffId"] = new SelectList(staffList, "Id", "FullName", maintenanceRequest.AssignedStaffId);

            PopulateAllUnits();
            return View(maintenanceRequest);
        }

        // GET: MaintenanceRequests/Delete/5
        [Authorize(Roles = "PropertyManager")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var maintenanceRequest = await _context.MaintenanceRequests
                .Include(m => m.Property)
                .Include(m => m.Unit)
                .Include(m => m.Tenant)
                .Include(m => m.AssignedStaff)
                .FirstOrDefaultAsync(m => m.RequestId == id);

            if (maintenanceRequest == null) return NotFound();

            return View(maintenanceRequest);
        }

        // POST: MaintenanceRequests/Delete/5
        [Authorize(Roles = "PropertyManager")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var maintenanceRequest = await _context.MaintenanceRequests.FindAsync(id);
            if (maintenanceRequest != null)
            {
                _context.MaintenanceRequests.Remove(maintenanceRequest);
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Maintenance request deleted successfully!";
            return RedirectToAction(nameof(Index));
        }

        private bool MaintenanceRequestExists(int id)
        {
            return _context.MaintenanceRequests.Any(e => e.RequestId == id);
        }

        private void PopulateAllUnits()
        {
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
        private async Task<string?> ValidateUnitVsStandalone(MaintenanceRequest req)
        {
            var property = await _context.Properties
                .Include(p => p.Units)
                .FirstOrDefaultAsync(p => p.PropertyId == req.PropertyId);
            if (property == null) return "Selected property does not exist.";

            bool hasUnits = property.Units != null && property.Units.Any();

            if (hasUnits && !req.UnitId.HasValue)
                return "This is a multi-unit property — you must select a unit.";

            if (!hasUnits && req.UnitId.HasValue)
                return "This is a standalone property — do not select a unit.";

            if (req.UnitId.HasValue)
            {
                var unitBelongs = property.Units!.Any(u => u.UnitId == req.UnitId.Value);
                if (!unitBelongs) return "Selected unit does not belong to the selected property.";
            }

            return null;
        }

        // SignalR broadcast including unit number. Plan B13 / L12.
        // Uses a per-request group so clients can subscribe to specific tickets, but
        // we ALSO broadcast to "live-board" so the dashboard ticker keeps working.
        private async Task BroadcastStatusUpdate(MaintenanceRequest request)
        {
            var unitNumber = request.Unit?.UnitNumber ?? "";
            await _hubContext.Clients.Group($"request-{request.RequestId}").SendAsync(
                "ReceiveStatusUpdate",
                request.RequestId,
                request.Status.ToString(),
                request.Title,
                unitNumber);
            await _hubContext.Clients.Group("live-board").SendAsync(
                "ReceiveStatusUpdate",
                request.RequestId,
                request.Status.ToString(),
                request.Title,
                unitNumber);
        }
    }
}
