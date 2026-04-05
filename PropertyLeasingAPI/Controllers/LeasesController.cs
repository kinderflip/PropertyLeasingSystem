using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PropertyLeasingAPI.Data;
using PropertyLeasingAPI.Models;

namespace PropertyLeasingAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LeasesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public LeasesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Leases
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Lease>>> GetLeases()
        {
            return await _context.Leases
                .Include(l => l.Property)
                .Include(l => l.Tenant)
                .ToListAsync();
        }

        // GET: api/Leases/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Lease>> GetLease(int id)
        {
            var lease = await _context.Leases
                .Include(l => l.Property)
                .Include(l => l.Tenant)
                .FirstOrDefaultAsync(l => l.LeaseId == id);

            if (lease == null) return NotFound();
            return lease;
        }

        // GET: api/Leases/active
        [HttpGet("active")]
        public async Task<ActionResult<IEnumerable<Lease>>> GetActiveLeases()
        {
            return await _context.Leases
                .Include(l => l.Property)
                .Include(l => l.Tenant)
                .Where(l => l.Status == LeaseStatus.Active)
                .ToListAsync();
        }

        // POST: api/Leases
        [HttpPost]
        public async Task<ActionResult<Lease>> PostLease(Lease lease)
        {
            _context.Leases.Add(lease);

            // Update property status to Leased
            var property = await _context.Properties.FindAsync(lease.PropertyId);
            if (property != null)
                property.Status = PropertyStatus.Leased;

            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetLease),
                new { id = lease.LeaseId }, lease);
        }

        // PUT: api/Leases/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutLease(int id, Lease lease)
        {
            if (id != lease.LeaseId) return BadRequest();

            _context.Entry(lease).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Leases.AnyAsync(l => l.LeaseId == id))
                    return NotFound();
                throw;
            }

            return NoContent();
        }

        // DELETE: api/Leases/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLease(int id)
        {
            var lease = await _context.Leases.FindAsync(id);
            if (lease == null) return NotFound();

            _context.Leases.Remove(lease);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}