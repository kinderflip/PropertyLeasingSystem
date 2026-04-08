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
                .Include(m => m.AssignedStaff)
                .ToListAsync();
        }

        // GET: api/MaintenanceRequests/5
        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<ActionResult<MaintenanceRequest>> GetMaintenanceRequest(int id)
        {
            var request = await _context.MaintenanceRequests
                .Include(m => m.Property)
                .Include(m => m.Tenant)
                .Include(m => m.AssignedStaff)
                .FirstOrDefaultAsync(m => m.RequestId == id);

            if (request == null) return NotFound();
            return request;
        }

        // GET: api/MaintenanceRequests/track?ticketId=5&phone=+97333112233 - Public, no auth required
        [AllowAnonymous]
        [HttpGet("track")]
        public async Task<ActionResult<object>> TrackRequest([FromQuery] int ticketId, [FromQuery] string phone)
        {
            if (ticketId <= 0 || string.IsNullOrWhiteSpace(phone))
                return BadRequest(new { message = "Both ticket number and phone number are required." });

            var request = await _context.MaintenanceRequests
                .Include(m => m.Property)
                .Include(m => m.Tenant)
                .Include(m => m.AssignedStaff)
                .FirstOrDefaultAsync(m => m.RequestId == ticketId);

            if (request == null)
                return NotFound(new { message = "No maintenance request found with that ticket number." });

            // Verify phone number matches the tenant who submitted the request
            if (request.Tenant == null || !request.Tenant.Phone.Replace(" ", "").Equals(phone.Replace(" ", ""), StringComparison.OrdinalIgnoreCase))
                return NotFound(new { message = "No matching request found. Please verify your ticket number and phone number." });

            return Ok(new
            {
                requestId = request.RequestId,
                title = request.Title,
                category = request.Category.ToString(),
                priority = request.Priority.ToString(),
                status = request.Status.ToString(),
                assignedTo = request.AssignedStaff?.FullName ?? "Unassigned",
                dateSubmitted = request.DateSubmitted.ToString("dd MMM yyyy"),
                dateAssigned = request.DateAssigned.HasValue
                    ? request.DateAssigned.Value.ToString("dd MMM yyyy")
                    : "Not assigned yet",
                dateResolved = request.DateResolved.HasValue
                    ? request.DateResolved.Value.ToString("dd MMM yyyy")
                    : "Not resolved yet",
                propertyAddress = request.Property?.Address,
                propertyCity = request.Property?.City
            });
        }

        // GET: api/MaintenanceRequests/pending
        [HttpGet("pending")]
        public async Task<ActionResult<IEnumerable<MaintenanceRequest>>> GetPendingRequests()
        {
            return await _context.MaintenanceRequests
                .Include(m => m.Property)
                .Include(m => m.Tenant)
                .Include(m => m.AssignedStaff)
                .Where(m => m.Status == MaintenanceStatus.Submitted || m.Status == MaintenanceStatus.Assigned)
                .ToListAsync();
        }

        // POST: api/MaintenanceRequests
        [HttpPost]
        [Authorize(Roles = "PropertyManager,Tenant")]
        public async Task<ActionResult<MaintenanceRequest>> PostMaintenanceRequest(MaintenanceRequest request)
        {
            request.DateSubmitted = DateTime.Now;
            request.Status = MaintenanceStatus.Submitted;
            _context.MaintenanceRequests.Add(request);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetMaintenanceRequest),
                new { id = request.RequestId }, request);
        }

        // PUT: api/MaintenanceRequests/5
        [HttpPut("{id}")]
        [Authorize(Roles = "PropertyManager,MaintenanceStaff")]
        public async Task<IActionResult> PutMaintenanceRequest(int id, MaintenanceRequest request)
        {
            if (id != request.RequestId) return BadRequest();

            // Auto-set dates based on status transitions
            if (request.Status == MaintenanceStatus.Assigned && request.DateAssigned == null)
                request.DateAssigned = DateTime.Now;

            if ((request.Status == MaintenanceStatus.Resolved || request.Status == MaintenanceStatus.Closed)
                && request.DateResolved == null)
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
        [Authorize(Roles = "PropertyManager")]
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
