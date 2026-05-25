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
    public class LeasesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public LeasesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Leases
        // PropertyManager sees all; Tenant sees only their own; everyone else 403.
        // Returns a clean projection (no back-references) so JSON stays small and well-formed.
        [HttpGet]
        public async Task<IActionResult> GetLeases()
        {
            var baseQuery = _context.Leases.AsQueryable();

            if (User.IsInRole("Tenant") && !User.IsInRole("PropertyManager"))
            {
                var myTenantId = await GetCallerTenantId();
                if (myTenantId == null) return Ok(new List<object>());
                baseQuery = baseQuery.Where(l => l.TenantId == myTenantId);
            }
            else if (!User.IsInRole("PropertyManager"))
            {
                return Forbid();
            }

            var leases = await baseQuery
                .Select(l => new
                {
                    leaseId = l.LeaseId,
                    propertyId = l.PropertyId,
                    unitId = l.UnitId,
                    tenantId = l.TenantId,
                    startDate = l.StartDate,
                    endDate = l.EndDate,
                    monthlyRent = l.MonthlyRent,
                    status = l.Status,
                    applicationDate = l.ApplicationDate,
                    applicationNotes = l.ApplicationNotes,
                    screeningNotes = l.ScreeningNotes,
                    approvalDate = l.ApprovalDate,
                    property = l.Property == null ? null : new
                    {
                        propertyId = l.Property.PropertyId,
                        address = l.Property.Address,
                        city = l.Property.City,
                        propertyType = l.Property.PropertyType,
                        status = l.Property.Status
                    },
                    unit = l.Unit == null ? null : new
                    {
                        unitId = l.Unit.UnitId,
                        unitNumber = l.Unit.UnitNumber,
                        monthlyRent = l.Unit.MonthlyRent
                    },
                    tenant = l.Tenant == null ? null : new
                    {
                        tenantId = l.Tenant.TenantId,
                        fullName = l.Tenant.FullName,
                        email = l.Tenant.Email,
                        phone = l.Tenant.Phone
                    }
                })
                .ToListAsync();

            return Ok(leases);
        }

        // GET: api/Leases/5
        // C3: caller must own the lease (or be a PropertyManager).
        [HttpGet("{id}")]
        public async Task<ActionResult<Lease>> GetLease(int id)
        {
            var lease = await _context.Leases
                .Include(l => l.Property)
                .Include(l => l.Unit)
                .Include(l => l.Tenant)
                .FirstOrDefaultAsync(l => l.LeaseId == id);

            if (lease == null) return NotFound();

            if (!User.IsInRole("PropertyManager"))
            {
                var myTenantId = await GetCallerTenantId();
                if (myTenantId == null || lease.TenantId != myTenantId) return Forbid();
            }

            return lease;
        }

        // GET: api/Leases/active — manager-only oversight view.
        [HttpGet("active")]
        [Authorize(Roles = "PropertyManager")]
        public async Task<ActionResult<IEnumerable<Lease>>> GetActiveLeases()
        {
            return await _context.Leases
                .Include(l => l.Property)
                .Include(l => l.Unit)
                .Include(l => l.Tenant)
                .Where(l => l.Status == LeaseStatus.Active)
                .ToListAsync();
        }

        // GET: api/Leases/applications — manager-only triage view.
        [HttpGet("applications")]
        [Authorize(Roles = "PropertyManager")]
        public async Task<ActionResult<IEnumerable<Lease>>> GetLeaseApplications()
        {
            return await _context.Leases
                .Include(l => l.Property)
                .Include(l => l.Unit)
                .Include(l => l.Tenant)
                .Where(l => l.Status == LeaseStatus.Application || l.Status == LeaseStatus.Screening)
                .ToListAsync();
        }

        // POST: api/Leases
        [HttpPost]
        [Authorize(Roles = "PropertyManager,Tenant")]
        public async Task<ActionResult<Lease>> PostLease(Lease lease)
        {
            // L1: auto-fill MonthlyRent BEFORE [Range(0.01, ...)] can reject a 0 value.
            if (lease.MonthlyRent <= 0)
            {
                if (lease.UnitId.HasValue)
                {
                    var unit = await _context.Units
                        .FirstOrDefaultAsync(u => u.UnitId == lease.UnitId.Value);
                    if (unit != null) lease.MonthlyRent = unit.MonthlyRent;
                }
                else
                {
                    var prop = await _context.Properties
                        .FirstOrDefaultAsync(p => p.PropertyId == lease.PropertyId);
                    if (prop?.MonthlyRent != null) lease.MonthlyRent = prop.MonthlyRent.Value;
                }
                if (lease.MonthlyRent > 0)
                    ModelState.Remove(nameof(Lease.MonthlyRent));
            }

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

            bool allowed = false;
            if (lease.Status == LeaseStatus.Application && newStatus == LeaseStatus.Screening) allowed = true;
            else if (lease.Status == LeaseStatus.Screening && (newStatus == LeaseStatus.Approved || newStatus == LeaseStatus.Rejected)) allowed = true;
            else if (lease.Status == LeaseStatus.Approved && newStatus == LeaseStatus.Active) allowed = true;
            else if (lease.Status == LeaseStatus.Active && (newStatus == LeaseStatus.Renewal || newStatus == LeaseStatus.Terminated || newStatus == LeaseStatus.Expired)) allowed = true;
            else if (lease.Status == LeaseStatus.Renewal && (newStatus == LeaseStatus.Active || newStatus == LeaseStatus.Terminated)) allowed = true;

            if (!allowed)
                return BadRequest(new { error = $"Invalid transition {lease.Status} -> {newStatus}." });

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
