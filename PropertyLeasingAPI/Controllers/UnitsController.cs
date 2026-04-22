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
    public class UnitsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UnitsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Units
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Unit>>> GetUnits()
        {
            return await _context.Units
                .Include(u => u.Property)
                .AsNoTracking()
                .ToListAsync();
        }

        // GET: api/Units/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Unit>> GetUnit(int id)
        {
            var unit = await _context.Units
                .Include(u => u.Property)
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.UnitId == id);

            if (unit == null) return NotFound();
            return unit;
        }

        // GET: api/Units/property/5 — units for a specific property
        [HttpGet("property/{propertyId}")]
        public async Task<ActionResult<IEnumerable<Unit>>> GetUnitsByProperty(int propertyId)
        {
            var exists = await _context.Properties.AnyAsync(p => p.PropertyId == propertyId);
            if (!exists) return NotFound(new { error = $"Property {propertyId} not found." });

            return await _context.Units
                .Where(u => u.PropertyId == propertyId)
                .AsNoTracking()
                .ToListAsync();
        }

        // GET: api/Units/available — all units currently available for lease
        [HttpGet("available")]
        public async Task<ActionResult<IEnumerable<Unit>>> GetAvailableUnits()
        {
            return await _context.Units
                .Include(u => u.Property)
                .Where(u => u.Status == UnitStatus.Available)
                .AsNoTracking()
                .ToListAsync();
        }

        // POST: api/Units
        [HttpPost]
        [Authorize(Roles = "PropertyManager")]
        public async Task<ActionResult<Unit>> PostUnit(Unit unit)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var parent = await _context.Properties.FindAsync(unit.PropertyId);
            if (parent == null)
                return BadRequest(new { error = $"Property {unit.PropertyId} not found." });

            // Reject adding a Unit to a property currently set up as standalone-leased.
            // A property is "standalone-leased" when it has Status != null and has no existing units.
            var hasExistingUnits = await _context.Units.AnyAsync(u => u.PropertyId == unit.PropertyId);
            if (!hasExistingUnits && parent.Status == PropertyStatus.Leased)
            {
                return BadRequest(new
                {
                    error = "Cannot add a unit to a property that is currently leased as a standalone. End the standalone lease first, or clear Property.Status."
                });
            }

            // Uniqueness: UnitNumber must be unique within a Property
            var duplicate = await _context.Units.AnyAsync(u =>
                u.PropertyId == unit.PropertyId && u.UnitNumber == unit.UnitNumber);
            if (duplicate)
            {
                ModelState.AddModelError(nameof(unit.UnitNumber),
                    $"Unit number '{unit.UnitNumber}' already exists for this property.");
                return BadRequest(ModelState);
            }

            _context.Units.Add(unit);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetUnit), new { id = unit.UnitId }, unit);
        }

        // PUT: api/Units/5
        [HttpPut("{id}")]
        [Authorize(Roles = "PropertyManager")]
        public async Task<IActionResult> PutUnit(int id, Unit unit)
        {
            if (id != unit.UnitId) return BadRequest();
            if (!ModelState.IsValid) return BadRequest(ModelState);

            // Uniqueness check (excluding self)
            var duplicate = await _context.Units.AnyAsync(u =>
                u.PropertyId == unit.PropertyId
                && u.UnitNumber == unit.UnitNumber
                && u.UnitId != id);
            if (duplicate)
            {
                ModelState.AddModelError(nameof(unit.UnitNumber),
                    $"Unit number '{unit.UnitNumber}' already exists for this property.");
                return BadRequest(ModelState);
            }

            _context.Entry(unit).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Units.AnyAsync(u => u.UnitId == id))
                    return NotFound();
                throw;
            }

            return NoContent();
        }

        // DELETE: api/Units/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "PropertyManager")]
        public async Task<IActionResult> DeleteUnit(int id)
        {
            var unit = await _context.Units.FindAsync(id);
            if (unit == null) return NotFound();

            // Protect against deleting a unit with active leases
            var hasActiveLease = await _context.Leases.AnyAsync(l =>
                l.UnitId == id &&
                (l.Status == LeaseStatus.Active || l.Status == LeaseStatus.Approved));
            if (hasActiveLease)
            {
                return BadRequest(new
                {
                    error = "Cannot delete a unit with an active or approved lease. Terminate the lease first."
                });
            }

            _context.Units.Remove(unit);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
