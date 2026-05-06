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
    public class PaymentsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IHubContext<NotificationHub> _notificationHub;

        public PaymentsController(AppDbContext context, IHubContext<NotificationHub> notificationHub)
        {
            _context = context;
            _notificationHub = notificationHub;
        }

        // M5: tenant-scoped view of own payments.
        [Authorize(Roles = Roles.Tenant + "," + Roles.PropertyManager)]
        public async Task<IActionResult> MyPayments(int page = 1)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var emptyPage = new PaginatedList<Payment>(new List<Payment>(), 0, 1, 20);

            if (string.IsNullOrEmpty(userId)) return View("Index", emptyPage);

            var tenant = await _context.Tenants.FirstOrDefaultAsync(t => t.UserId == userId);
            if (tenant == null) return View("Index", emptyPage);

            var today = DateTime.Today;
            var overdue = await _context.Payments
                .Where(p => p.Status == PaymentStatus.Pending && p.DueDate.Date < today
                            && p.Lease!.TenantId == tenant.TenantId)
                .ToListAsync();
            foreach (var p in overdue) p.Status = PaymentStatus.Overdue;
            if (overdue.Count > 0) await _context.SaveChangesAsync();

            var query = _context.Payments
                .Include(p => p.Lease).ThenInclude(l => l!.Property)
                .Include(p => p.Lease).ThenInclude(l => l!.Tenant)
                .Where(p => p.Lease!.TenantId == tenant.TenantId)
                .OrderByDescending(p => p.DueDate);

            ViewBag.MyView = true;
            return View("Index", await PaginatedList<Payment>.CreateAsync(query, page, 20));
        }

        // GET: Payments
        public async Task<IActionResult> Index(string? searchString, PaymentStatus? status, PaymentType? type, int page = 1)
        {
            // Auto-flag overdue payments (normalise DueDate to date-only to avoid off-by-one)
            var today = DateTime.Today;
            var overduePayments = await _context.Payments
                .Where(p => p.Status == PaymentStatus.Pending && p.DueDate.Date < today)
                .ToListAsync();

            if (overduePayments.Any())
            {
                foreach (var p in overduePayments)
                    p.Status = PaymentStatus.Overdue;
                await _context.SaveChangesAsync();
            }

            var payments = _context.Payments
                .Include(p => p.Lease)
                    .ThenInclude(l => l!.Property)
                .Include(p => p.Lease)
                    .ThenInclude(l => l!.Tenant)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
                payments = payments.Where(p =>
                    p.Lease!.Property!.Address.Contains(searchString) ||
                    p.Lease!.Tenant!.FullName.Contains(searchString));

            if (status.HasValue)
                payments = payments.Where(p => p.Status == status);

            if (type.HasValue)
                payments = payments.Where(p => p.PaymentType == type);

            ViewBag.SearchString = searchString;
            ViewBag.Status = status;
            ViewBag.Type = type;

            return View(await PaginatedList<Payment>.CreateAsync(
                payments.OrderByDescending(p => p.DueDate), page, 20));
        }

        // GET: Payments/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var payment = await _context.Payments
                .Include(p => p.Lease)
                    .ThenInclude(l => l!.Property)
                .Include(p => p.Lease)
                    .ThenInclude(l => l!.Tenant)
                .FirstOrDefaultAsync(m => m.PaymentId == id);

            if (payment == null) return NotFound();

            return View(payment);
        }

        // GET: Payments/Create
        [Authorize(Roles = Roles.PropertyManager)]
        public IActionResult Create()
        {
            ViewData["LeaseId"] = new SelectList(
                _context.Leases
                    .Include(l => l.Property)
                    .Include(l => l.Tenant)
                    .Where(l => l.Status == LeaseStatus.Active)
                    .Select(l => new { l.LeaseId, Display = l.Tenant!.FullName + " - " + l.Property!.Address }),
                "LeaseId", "Display");
            return View();
        }

        // POST: Payments/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = Roles.PropertyManager)]
        public async Task<IActionResult> Create([Bind("PaymentId,LeaseId,Amount,DueDate,PaymentDate,PaymentType,Status")] Payment payment)
        {
            if (ModelState.IsValid)
            {
                // If payment date is set, mark as completed
                if (payment.PaymentDate.HasValue)
                    payment.Status = PaymentStatus.Completed;

                _context.Add(payment);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Payment created successfully!";
                return RedirectToAction(nameof(Index));
            }
            ViewData["LeaseId"] = new SelectList(
                _context.Leases
                    .Include(l => l.Property)
                    .Include(l => l.Tenant)
                    .Where(l => l.Status == LeaseStatus.Active)
                    .Select(l => new { l.LeaseId, Display = l.Tenant!.FullName + " - " + l.Property!.Address }),
                "LeaseId", "Display", payment.LeaseId);
            return View(payment);
        }

        // POST: Payments/MarkAsPaid/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = Roles.PropertyManager)]
        public async Task<IActionResult> MarkAsPaid(int id)
        {
            var payment = await _context.Payments.FindAsync(id);
            if (payment == null) return NotFound();

            payment.Status = PaymentStatus.Completed;
            payment.PaymentDate = DateTime.Today;
            await _context.SaveChangesAsync();

            // Notify tenant
            var lease = await _context.Leases.Include(l => l.Tenant).FirstOrDefaultAsync(l => l.LeaseId == payment.LeaseId);
            if (lease?.Tenant?.UserId != null)
            {
                await NotificationsController.CreateNotification(_context,
                    lease.Tenant.UserId,
                    "Payment Received",
                    $"Your payment of {payment.Amount:N3} BD has been marked as paid.",
                    NotificationType.PaymentReminder,
                    $"/Payments/Details/{payment.PaymentId}",
                    _notificationHub);
            }

            TempData["SuccessMessage"] = "Payment marked as paid!";
            return RedirectToAction(nameof(Details), new { id });
        }

        // GET: Payments/Edit/5
        [Authorize(Roles = Roles.PropertyManager)]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var payment = await _context.Payments.FindAsync(id);
            if (payment == null) return NotFound();

            ViewData["LeaseId"] = new SelectList(
                _context.Leases
                    .Include(l => l.Property)
                    .Include(l => l.Tenant)
                    .Select(l => new { l.LeaseId, Display = l.Tenant!.FullName + " - " + l.Property!.Address }),
                "LeaseId", "Display", payment.LeaseId);
            return View(payment);
        }

        // POST: Payments/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = Roles.PropertyManager)]
        public async Task<IActionResult> Edit(int id, [Bind("PaymentId,LeaseId,Amount,DueDate,PaymentDate,PaymentType,Status")] Payment payment)
        {
            if (id != payment.PaymentId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    // Auto-complete if payment date is set
                    if (payment.PaymentDate.HasValue && payment.Status == PaymentStatus.Pending)
                        payment.Status = PaymentStatus.Completed;

                    _context.Update(payment);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Payment updated successfully!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PaymentExists(payment.PaymentId))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["LeaseId"] = new SelectList(
                _context.Leases
                    .Include(l => l.Property)
                    .Include(l => l.Tenant)
                    .Select(l => new { l.LeaseId, Display = l.Tenant!.FullName + " - " + l.Property!.Address }),
                "LeaseId", "Display", payment.LeaseId);
            return View(payment);
        }

        // GET: Payments/Delete/5
        [Authorize(Roles = Roles.PropertyManager)]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var payment = await _context.Payments
                .Include(p => p.Lease)
                    .ThenInclude(l => l!.Property)
                .Include(p => p.Lease)
                    .ThenInclude(l => l!.Tenant)
                .FirstOrDefaultAsync(m => m.PaymentId == id);

            if (payment == null) return NotFound();

            return View(payment);
        }

        // POST: Payments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = Roles.PropertyManager)]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var payment = await _context.Payments.FindAsync(id);
            if (payment != null)
            {
                _context.Payments.Remove(payment);
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Payment deleted successfully!";
            return RedirectToAction(nameof(Index));
        }

        private bool PaymentExists(int id)
        {
            return _context.Payments.Any(e => e.PaymentId == id);
        }
    }
}
