using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PropertyLeasingAPI.Data;
using PropertyLeasingAPI.Models;

namespace PropertyLeasingAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PaymentsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public PaymentsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Payments
        // C3: PropertyManager sees all; Tenant sees only payments on their own leases.
        [HttpGet]
        public async Task<IActionResult> GetPayments()
        {
            await FlagOverduePayments();

            var query = _context.Payments.AsQueryable();

            if (User.IsInRole("Tenant") && !User.IsInRole("PropertyManager"))
            {
                var myTenantId = await GetCallerTenantId();
                if (myTenantId == null) return Ok(new List<object>());
                query = query.Where(p => p.Lease!.TenantId == myTenantId);
            }
            else if (!User.IsInRole("PropertyManager"))
            {
                return Forbid();
            }

            var payments = await query
                .OrderByDescending(p => p.DueDate)
                .Select(p => new
                {
                    paymentId = p.PaymentId,
                    leaseId = p.LeaseId,
                    amount = p.Amount,
                    dueDate = p.DueDate,
                    paymentDate = p.PaymentDate,
                    paymentType = (int)p.PaymentType,
                    status = (int)p.Status
                })
                .ToListAsync();

            return Ok(payments);
        }

        // GET: api/Payments/5
        // C3: caller must own the lease that owns the payment.
        [HttpGet("{id}")]
        public async Task<ActionResult<Payment>> GetPayment(int id)
        {
            var payment = await _context.Payments
                .Include(p => p.Lease)
                .FirstOrDefaultAsync(p => p.PaymentId == id);

            if (payment == null) return NotFound();

            if (!User.IsInRole("PropertyManager"))
            {
                var myTenantId = await GetCallerTenantId();
                if (myTenantId == null || payment.Lease?.TenantId != myTenantId) return Forbid();
            }

            return payment;
        }

        // GET: api/Payments/lease/5
        // C3: caller must own the lease (or be a PropertyManager).
        [HttpGet("lease/{leaseId}")]
        public async Task<ActionResult<IEnumerable<Payment>>> GetPaymentsByLease(int leaseId)
        {
            if (!User.IsInRole("PropertyManager"))
            {
                var myTenantId = await GetCallerTenantId();
                if (myTenantId == null) return Forbid();

                var owns = await _context.Leases
                    .AnyAsync(l => l.LeaseId == leaseId && l.TenantId == myTenantId);
                if (!owns) return Forbid();
            }

            return await _context.Payments
                .Where(p => p.LeaseId == leaseId)
                .OrderByDescending(p => p.DueDate)
                .ToListAsync();
        }

        // GET: api/Payments/overdue — manager-only oversight view.
        [HttpGet("overdue")]
        [Authorize(Roles = "PropertyManager")]
        public async Task<ActionResult<IEnumerable<Payment>>> GetOverduePayments()
        {
            await FlagOverduePayments();

            return await _context.Payments
                .Include(p => p.Lease)
                    .ThenInclude(l => l!.Tenant)
                .Where(p => p.Status == PaymentStatus.Overdue)
                .OrderBy(p => p.DueDate)
                .ToListAsync();
        }

        // POST: api/Payments
        [HttpPost]
        [Authorize(Roles = "PropertyManager")]
        public async Task<ActionResult<Payment>> PostPayment(Payment payment)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetPayment),
                new { id = payment.PaymentId }, payment);
        }

        // DELETE: api/Payments/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "PropertyManager")]
        public async Task<IActionResult> DeletePayment(int id)
        {
            var payment = await _context.Payments.FindAsync(id);
            if (payment == null) return NotFound();

            _context.Payments.Remove(payment);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // Auto-flag pending payments past their due date as Overdue.
        // B7: compare on Date component to avoid off-by-one when DueDate has a time stamp.
        private async Task FlagOverduePayments()
        {
            var today = DateTime.Today;
            var overduePayments = await _context.Payments
                .Where(p => p.Status == PaymentStatus.Pending && p.DueDate < today)
                .ToListAsync();

            if (overduePayments.Count > 0)
            {
                foreach (var p in overduePayments)
                    p.Status = PaymentStatus.Overdue;

                await _context.SaveChangesAsync();
            }
        }

        // C3: resolve the caller's TenantId via their ApplicationUser.Id claim.
        private async Task<int?> GetCallerTenantId()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return null;
            return await _context.Tenants
                .Where(t => t.UserId == userId)
                .Select(t => (int?)t.TenantId)
                .FirstOrDefaultAsync();
        }
    }
}
