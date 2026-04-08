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

        // GET: api/Leases/applications
        [HttpGet("applications")]
        public async Task<ActionResult<IEnumerable<Lease>>> GetLeaseApplications()
        {
            return await _context.Leases
                .Include(l => l.Property)
                .Include(l => l.Tenant)
                .Where(l => l.Status == LeaseStatus.Application || l.Status == LeaseStatus.Screening)
                .ToListAsync();
        }

        // POST: api/Leases
        [HttpPost]
        [Authorize(Roles = "PropertyManager,Tenant")]
        public async Task<ActionResult<Lease>> PostLease(Lease lease)
        {
            // New leases start as Application
            lease.Status = LeaseStatus.Application;
            lease.ApplicationDate = DateTime.Now;

            _context.Leases.Add(lease);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetLease),
                new { id = lease.LeaseId }, lease);
        }

        // PUT: api/Leases/5
        [HttpPut("{id}")]
        [Authorize(Roles = "PropertyManager")]
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
        [Authorize(Roles = "PropertyManager")]
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
