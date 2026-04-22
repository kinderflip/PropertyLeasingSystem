using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PropertyLeasingAPI.Data;
using PropertyLeasingAPI.Models;
using PropertyLeasingAPI.Services;

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
                .Include(l => l.Unit)
                .Include(l => l.Tenant)
                .AsNoTracking()
                .ToListAsync();
        }

        // GET: api/Leases/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Lease>> GetLease(int id)
        {
            var lease = await _context.Leases
                .Include(l => l.Property)
                .Include(l => l.Unit)
                .Include(l => l.Tenant)
                .AsNoTracking()
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
                .Include(l => l.Unit)
                .Include(l => l.Tenant)
                .Where(l => l.Status == LeaseStatus.Active)
                .AsNoTracking()
                .ToListAsync();
        }

        // GET: api/Leases/applications
        [HttpGet("applications")]
        public async Task<ActionResult<IEnumerable<Lease>>> GetLeaseApplications()
        {
            return await _context.Leases
                .Include(l => l.Property)
                .Include(l => l.Unit)
                .Include(l => l.Tenant)
                .Where(l => l.Status == LeaseStatus.Application || l.Status == LeaseStatus.Screening)
                .AsNoTracking()
                .ToListAsync();
        }

        // POST: api/Leases
        [HttpPost]
        [Authorize(Roles = "PropertyManager,Tenant")]
        public async Task<ActionResult<Lease>> PostLease(Lease lease)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var unitRuleError = await ValidateUnitVsStandalone(lease);
            if (unitRuleError != null)
            {
                ModelState.AddModelError(nameof(lease.UnitId), unitRuleError);
                return BadRequest(ModelState);
            }

            if (await HasOverlap(lease))
            {
                ModelState.AddModelError(string.Empty,
                    "Another approved or active lease already covers this date range for the same unit/property.");
                return BadRequest(ModelState);
            }

            // Default MonthlyRent from the Unit/Property if caller omitted it.
            if (lease.MonthlyRent <= 0)
            {
                if (lease.UnitId.HasValue)
                {
                    var unit = await _context.Units.AsNoTracking()
                        .FirstOrDefaultAsync(u => u.UnitId == lease.UnitId.Value);
                    if (unit != null) lease.MonthlyRent = unit.MonthlyRent;
                }
                else
                {
                    var prop = await _context.Properties.AsNoTracking()
                        .FirstOrDefaultAsync(p => p.PropertyId == lease.PropertyId);
                    if (prop?.MonthlyRent != null) lease.MonthlyRent = prop.MonthlyRent.Value;
                }
            }

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
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var unitRuleError = await ValidateUnitVsStandalone(lease);
            if (unitRuleError != null)
            {
                ModelState.AddModelError(nameof(lease.UnitId), unitRuleError);
                return BadRequest(ModelState);
            }

            if (await HasOverlap(lease))
            {
                ModelState.AddModelError(string.Empty,
                    "Another approved or active lease already covers this date range for the same unit/property.");
                return BadRequest(ModelState);
            }

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

        // PUT: api/Leases/5/status — advance lease lifecycle through the state machine
        // B10.3: API now exposes status transitions (previously only MVC did).
        [HttpPut("{id}/status")]
        [Authorize(Roles = "PropertyManager")]
        public async Task<IActionResult> PutLeaseStatus(int id, [FromBody] LeaseStatus newStatus)
        {
            var lease = await _context.Leases.FindAsync(id);
            if (lease == null) return NotFound();

            if (!StatusTransitions.IsValidLeaseTransition(lease.Status, newStatus))
                return BadRequest(new { error = $"Invalid transition {lease.Status} → {newStatus}." });

            lease.Status = newStatus;
            if (newStatus == LeaseStatus.Approved) lease.ApprovalDate = DateTime.Now;

            await _context.SaveChangesAsync();
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

        // ---- helpers ----

        // Returns error message if the multi-unit-vs-standalone rule is violated, otherwise null.
        private async Task<string?> ValidateUnitVsStandalone(Lease lease)
        {
            var property = await _context.Properties
                .Include(p => p.Units)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.PropertyId == lease.PropertyId);

            if (property == null) return "Property not found.";

            var hasUnits = property.Units != null && property.Units.Count > 0;

            if (hasUnits && !lease.UnitId.HasValue)
                return "This property has units. You must select a unit.";

            if (!hasUnits && lease.UnitId.HasValue)
                return "This property is standalone. Clear the unit selection.";

            if (lease.UnitId.HasValue)
            {
                var unitBelongs = property.Units!.Any(u => u.UnitId == lease.UnitId.Value);
                if (!unitBelongs)
                    return "The selected unit does not belong to this property.";
            }

            return null;
        }

        // Date-range overlap check, multi-unit aware.
        private async Task<bool> HasOverlap(Lease lease)
        {
            return await _context.Leases.AnyAsync(l =>
                l.LeaseId != lease.LeaseId &&
                (l.Status == LeaseStatus.Active || l.Status == LeaseStatus.Approved) &&
                l.StartDate <= lease.EndDate &&
                l.EndDate >= lease.StartDate &&
                (
                    // Multi-unit: clash on the same unit
                    (lease.UnitId != null && l.UnitId == lease.UnitId)
                    ||
                    // Standalone: clash on the same property and neither side has a unit
                    (lease.UnitId == null && l.UnitId == null && l.PropertyId == lease.PropertyId)
                )
            );
        }
    }
}
