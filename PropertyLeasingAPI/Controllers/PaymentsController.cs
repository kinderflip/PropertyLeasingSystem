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
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Payment>>> GetPayments()
        {
            // Auto-flag overdue payments
            await FlagOverduePayments();

            return await _context.Payments
                .Include(p => p.Lease)
                .OrderByDescending(p => p.DueDate)
                .ToListAsync();
        }

        // GET: api/Payments/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Payment>> GetPayment(int id)
        {
            var payment = await _context.Payments
                .Include(p => p.Lease)
                .FirstOrDefaultAsync(p => p.PaymentId == id);

            if (payment == null) return NotFound();
            return payment;
        }

        // GET: api/Payments/lease/5
        [HttpGet("lease/{leaseId}")]
        public async Task<ActionResult<IEnumerable<Payment>>> GetPaymentsByLease(int leaseId)
        {
            return await _context.Payments
                .Where(p => p.LeaseId == leaseId)
                .OrderByDescending(p => p.DueDate)
                .ToListAsync();
        }

        // GET: api/Payments/overdue
        [HttpGet("overdue")]
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
    }
}
