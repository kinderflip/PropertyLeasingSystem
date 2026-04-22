using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PropertyLeasingAPI.Data;
using PropertyLeasingAPI.Models;

namespace PropertyLeasingMVC.Controllers
{
    [Authorize]
    public class UnitsController : Controller
    {
        private readonly AppDbContext _context;

        public UnitsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Units?propertyId=1
        public async Task<IActionResult> Index(int? propertyId, UnitStatus? status, UnitType? type)
        {
            var units = _context.Units.Include(u => u.Property).AsQueryable();

            if (propertyId.HasValue)
                units = units.Where(u => u.PropertyId == propertyId.Value);

            if (status.HasValue)
                units = units.Where(u => u.Status == status.Value);

            if (type.HasValue)
                units = units.Where(u => u.UnitType == type.Value);

            ViewBag.PropertyId = propertyId;
            ViewBag.Status = status;
            ViewBag.Type = type;

            if (propertyId.HasValue)
            {
                var prop = await _context.Properties
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.PropertyId == propertyId.Value);
                ViewBag.PropertyName = prop == null ? null : $"{prop.Address}, {prop.City}";
            }

            return View(await units.AsNoTracking().ToListAsync());
        }

        // GET: Units/Browse — public-facing listing for tenants to browse available units
        [AllowAnonymous]
        public async Task<IActionResult> Browse(string? city, UnitType? type, decimal? maxRent)
        {
            var q = _context.Units
                .Include(u => u.Property)
                .Where(u => u.Status == UnitStatus.Available);

            if (!string.IsNullOrWhiteSpace(city))
                q = q.Where(u => u.Property!.City.Contains(city));

            if (type.HasValue)
                q = q.Where(u => u.UnitType == type.Value);

            if (maxRent.HasValue)
                q = q.Where(u => u.MonthlyRent <= maxRent.Value);

            ViewBag.City = city;
            ViewBag.Type = type;
            ViewBag.MaxRent = maxRent;

            return View(await q.AsNoTracking().ToListAsync());
        }

        // GET: Units/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var unit = await _context.Units
                .Include(u => u.Property)
                .Include(u => u.Leases)!.ThenInclude(l => l.Tenant)
                .Include(u => u.MaintenanceRequests)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.UnitId == id);

            if (unit == null) return NotFound();
            return View(unit);
        }

        // Units/ForPropertyJson?propertyId=1 — AJAX feed for the cascade dropdown
        [HttpGet]
        public async Task<IActionResult> ForPropertyJson(int propertyId)
        {
            var units = await _context.Units
                .Where(u => u.PropertyId == propertyId)
                .OrderBy(u => u.UnitNumber)
                .Select(u => new
                {
                    unitId = u.UnitId,
                    unitNumber = u.UnitNumber,
                    unitType = u.UnitType.ToString(),
                    monthlyRent = u.MonthlyRent,
                    status = u.Status.ToString()
                })
                .AsNoTracking()
                .ToListAsync();

            return Json(units);
        }

        // GET: Units/Create?propertyId=1
        [Authorize(Roles = "PropertyManager")]
        public async Task<IActionResult> Create(int? propertyId)
        {
            await PopulatePropertyDropdown(propertyId);
            return View(new Unit
            {
                UnitNumber = string.Empty,
                PropertyId = propertyId ?? 0
            });
        }

        // POST: Units/Create
        [Authorize(Roles = "PropertyManager")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("UnitId,PropertyId,UnitNumber,UnitType,Amenities,SizeSqm,MonthlyRent,Status,Description")] Unit unit)
        {
            var parent = await _context.Properties.FindAsync(unit.PropertyId);
            if (parent == null)
            {
                ModelState.AddModelError(nameof(unit.PropertyId), "Selected property does not exist.");
            }
            else
            {
                var hasUnits = await _context.Units.AnyAsync(u => u.PropertyId == unit.PropertyId);
                if (!hasUnits && parent.Status == PropertyStatus.Leased)
                {
                    ModelState.AddModelError(string.Empty,
                        "Cannot add a unit to a property currently leased as standalone. End the lease first.");
                }

                var duplicate = await _context.Units.AnyAsync(u =>
                    u.PropertyId == unit.PropertyId && u.UnitNumber == unit.UnitNumber);
                if (duplicate)
                    ModelState.AddModelError(nameof(unit.UnitNumber),
                        $"Unit number '{unit.UnitNumber}' already exists for this property.");
            }

            if (ModelState.IsValid)
            {
                _context.Add(unit);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Unit {unit.UnitNumber} created successfully!";
                return RedirectToAction(nameof(Index), new { propertyId = unit.PropertyId });
            }

            await PopulatePropertyDropdown(unit.PropertyId);
            return View(unit);
        }

        // GET: Units/Edit/5
        [Authorize(Roles = "PropertyManager")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var unit = await _context.Units.FindAsync(id);
            if (unit == null) return NotFound();

            await PopulatePropertyDropdown(unit.PropertyId);
            return View(unit);
        }

        // POST: Units/Edit/5
        [Authorize(Roles = "PropertyManager")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("UnitId,PropertyId,UnitNumber,UnitType,Amenities,SizeSqm,MonthlyRent,Status,Description")] Unit unit)
        {
            if (id != unit.UnitId) return NotFound();

            var duplicate = await _context.Units.AnyAsync(u =>
                u.PropertyId == unit.PropertyId
                && u.UnitNumber == unit.UnitNumber
                && u.UnitId != id);
            if (duplicate)
                ModelState.AddModelError(nameof(unit.UnitNumber),
                    $"Unit number '{unit.UnitNumber}' already exists for this property.");

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(unit);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = $"Unit {unit.UnitNumber} updated successfully!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _context.Units.AnyAsync(u => u.UnitId == id)) return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index), new { propertyId = unit.PropertyId });
            }

            await PopulatePropertyDropdown(unit.PropertyId);
            return View(unit);
        }

        // GET: Units/Delete/5
        [Authorize(Roles = "PropertyManager")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var unit = await _context.Units
                .Include(u => u.Property)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.UnitId == id);

            if (unit == null) return NotFound();
            return View(unit);
        }

        // POST: Units/Delete/5
        [Authorize(Roles = "PropertyManager")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var unit = await _context.Units.FindAsync(id);
            if (unit == null) return NotFound();

            var hasActiveLease = await _context.Leases.AnyAsync(l =>
                l.UnitId == id &&
                (l.Status == LeaseStatus.Active || l.Status == LeaseStatus.Approved));
            if (hasActiveLease)
            {
                TempData["ErrorMessage"] = "Cannot delete a unit with an active or approved lease.";
                return RedirectToAction(nameof(Details), new { id });
            }

            var propertyId = unit.PropertyId;
            _context.Units.Remove(unit);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Unit deleted successfully!";
            return RedirectToAction(nameof(Index), new { propertyId });
        }

        private async Task PopulatePropertyDropdown(int? selectedId)
        {
            var props = await _context.Properties
                .OrderBy(p => p.Address)
                .Select(p => new { p.PropertyId, Label = p.Address + ", " + p.City })
                .AsNoTracking()
                .ToListAsync();
            ViewData["PropertyId"] = new SelectList(props, "PropertyId", "Label", selectedId);
        }
    }
}
