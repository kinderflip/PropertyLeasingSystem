using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PropertyLeasingAPI.Data;
using PropertyLeasingAPI.Models;

namespace PropertyLeasingAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MaintenanceRequestsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public MaintenanceRequestsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/MaintenanceRequests
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MaintenanceRequest>>> GetMaintenanceRequests()
        {
            return await _context.MaintenanceRequests
                .Include(m => m.Property)
                .Include(m => m.Tenant)
                .ToListAsync();
        }

        // GET: api/MaintenanceRequests/5
        [HttpGet("{id}")]
        public async Task<ActionResult<MaintenanceRequest>> GetMaintenanceRequest(int id)
        {
            var request = await _context.MaintenanceRequests
                .Include(m => m.Property)
                .Include(m => m.Tenant)
                .FirstOrDefaultAsync(m => m.RequestId == id);

            if (request == null) return NotFound();
            return request;
        }

        // GET: api/MaintenanceRequests/pending
        [HttpGet("pending")]
        public async Task<ActionResult<IEnumerable<MaintenanceRequest>>> GetPendingRequests()
        {
            return await _context.MaintenanceRequests
                .Include(m => m.Property)
                .Include(m => m.Tenant)
                .Where(m => m.Status == MaintenanceStatus.Pending)
                .ToListAsync();
        }

        // POST: api/MaintenanceRequests
        [HttpPost]
        public async Task<ActionResult<MaintenanceRequest>> PostMaintenanceRequest(MaintenanceRequest request)
        {
            request.DateSubmitted = DateTime.Now;
            _context.MaintenanceRequests.Add(request);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetMaintenanceRequest),
                new { id = request.RequestId }, request);
        }

        // PUT: api/MaintenanceRequests/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutMaintenanceRequest(int id, MaintenanceRequest request)
        {
            if (id != request.RequestId) return BadRequest();

            // If being marked as completed, set resolved date
            if (request.Status == MaintenanceStatus.Completed && request.DateResolved == null)
                request.DateResolved = DateTime.Now;

            _context.Entry(request).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.MaintenanceRequests.AnyAsync(m => m.RequestId == id))
                    return NotFound();
                throw;
            }

            return NoContent();
        }

        // DELETE: api/MaintenanceRequests/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMaintenanceRequest(int id)
        {
            var request = await _context.MaintenanceRequests.FindAsync(id);
            if (request == null) return NotFound();

            _context.MaintenanceRequests.Remove(request);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}