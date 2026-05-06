using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using PropertyLeasingAPI.Data;
using PropertyLeasingAPI.Models;
using PropertyLeasingAPI.Services;
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
        public async Task<IActionResult> Index(string? searchString, MaintenanceStatus? status, MaintenanceCategory? category, MaintenancePriority? priority, int page = 1)
        {
            var requests = _context.MaintenanceRequests
                .Include(m => m.Property)
                .Include(m => m.Unit)
                .Include(m => m.Tenant)
                .Include(m => m.AssignedStaff)
                .AsQueryable();

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

            return View(await PaginatedList<MaintenanceRequest>.CreateAsync(
                requests.OrderByDescending(m => m.DateSubmitted), page, 20));
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

            // M1 + M10: ranked staff list — skill-matched first, then other available staff,
            // unavailable staff hidden by default (manager can still see them in Edit if needed).
            var allStaff = await _userManager.GetUsersInRoleAsync(Roles.MaintenanceStaff);
            var category = maintenanceRequest.Category.ToString();
            var available = allStaff.Where(u => u.IsAvailable).ToList();
            var skilled = available.Where(u => !string.IsNullOrEmpty(u.Skills)
                                            && u.Skills.Split(',', StringSplitOptions.TrimEntries)
                                                       .Contains(category, StringComparer.OrdinalIgnoreCase)).ToList();
            var ranked = skilled.Concat(available.Except(skilled)).ToList();
            ViewBag.StaffList = new SelectList(ranked, "Id", "FullName", maintenanceRequest.AssignedStaffId);

            return View(maintenanceRequest);
        }

        // POST: MaintenanceRequests/Assign/5 - Property Manager assigns staff
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = Roles.PropertyManager)]
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
        [Authorize(Roles = Roles.PropertyManager + "," + Roles.MaintenanceStaff)]
        public async Task<IActionResult> UpdateStatus(int id, MaintenanceStatus newStatus, string? staffNotes)
        {
            var request = await _context.MaintenanceRequests
                .Include(m => m.Unit)
                .FirstOrDefaultAsync(m => m.RequestId == id);
            if (request == null) return NotFound();

            // Shared rule set — same source of truth as API
            if (!StatusTransitions.IsValidMaintenanceTransition(request.Status, newStatus))
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
        [Authorize(Roles = Roles.PropertyManager + "," + Roles.Tenant)]
        public IActionResult Create()
        {
            ViewData["PropertyId"] = new SelectList(_context.Properties, "PropertyId", "Address");
            ViewData["TenantId"] = new SelectList(_context.Tenants, "TenantId", "FullName");
            return View();
        }

        // POST: MaintenanceRequests/Create
        [Authorize(Roles = Roles.PropertyManager + "," + Roles.Tenant)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("RequestId,PropertyId,UnitId,TenantId,Title,Description,Category,Priority")] MaintenanceRequest maintenanceRequest)
        {
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
            return View(maintenanceRequest);
        }

        // GET: MaintenanceRequests/Edit/5
        [Authorize(Roles = Roles.PropertyManager + "," + Roles.MaintenanceStaff)]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var maintenanceRequest = await _context.MaintenanceRequests.FindAsync(id);
            if (maintenanceRequest == null) return NotFound();

            ViewData["PropertyId"] = new SelectList(_context.Properties, "PropertyId", "Address", maintenanceRequest.PropertyId);
            ViewData["TenantId"] = new SelectList(_context.Tenants, "TenantId", "FullName", maintenanceRequest.TenantId);

            var staffUsers = await _userManager.GetUsersInRoleAsync(Roles.MaintenanceStaff);
            ViewData["AssignedStaffId"] = new SelectList(staffUsers, "Id", "FullName", maintenanceRequest.AssignedStaffId);

            return View(maintenanceRequest);
        }

        // POST: MaintenanceRequests/Edit/5
        [Authorize(Roles = Roles.PropertyManager + "," + Roles.MaintenanceStaff)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("RequestId,PropertyId,UnitId,TenantId,Title,Description,Category,Priority,Status,AssignedStaffId,StaffNotes,DateSubmitted,DateAssigned,DateResolved")] MaintenanceRequest maintenanceRequest)
        {
            if (id != maintenanceRequest.RequestId) return NotFound();

            // L3: MaintenanceStaff are allowed to edit only Status + StaffNotes.
            // Reload everything else from DB so an over-posted form can't reassign tickets.
            if (User.IsInRole(Roles.MaintenanceStaff) && !User.IsInRole(Roles.PropertyManager))
            {
                var existing = await _context.MaintenanceRequests
                    .AsNoTracking()
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

            var staffList = await _userManager.GetUsersInRoleAsync(Roles.MaintenanceStaff);
            ViewData["AssignedStaffId"] = new SelectList(staffList, "Id", "FullName", maintenanceRequest.AssignedStaffId);

            return View(maintenanceRequest);
        }

        // GET: MaintenanceRequests/Delete/5
        [Authorize(Roles = Roles.PropertyManager)]
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
        [Authorize(Roles = Roles.PropertyManager)]
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
