using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PropertyLeasingAPI.Data;
using PropertyLeasingAPI.Models;
using Microsoft.AspNetCore.Authorization;

namespace PropertyLeasingMVC.Controllers
{
    [Authorize]
    public class PaymentsController : Controller
    {
        private readonly AppDbContext _context;

        public PaymentsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Payments
        public async Task<IActionResult> Index(string? searchString, PaymentStatus? status, PaymentType? type)
        {
            // Auto-flag overdue payments
            var overduePayments = await _context.Payments
                .Where(p => p.Status == PaymentStatus.Pending && p.DueDate < DateTime.Today)
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

            return View(await payments.OrderByDescending(p => p.DueDate).ToListAsync());
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
        [Authorize(Roles = "PropertyManager")]
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
        [Authorize(Roles = "PropertyManager")]
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
        [Authorize(Roles = "PropertyManager")]
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
                    $"Your payment of {payment.Amount:C} has been marked as paid.",
                    NotificationType.PaymentReminder,
                    $"/Payments/Details/{payment.PaymentId}");
            }

            TempData["SuccessMessage"] = "Payment marked as paid!";
            return RedirectToAction(nameof(Details), new { id });
        }

        // GET: Payments/Edit/5
        [Authorize(Roles = "PropertyManager")]
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
        [Authorize(Roles = "PropertyManager")]
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
        [Authorize(Roles = "PropertyManager")]
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
        [Authorize(Roles = "PropertyManager")]
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
