using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PropertyLeasingAPI.Data;
using PropertyLeasingAPI.Models;
using PropertyLeasingMVC.ViewModels;
using Microsoft.AspNetCore.Authorization;

namespace PropertyLeasingMVC.Controllers
{
    [Authorize]
    public class PropertiesController : Controller
    {
        private readonly AppDbContext _context;

        public PropertiesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Properties — M9: public so prospective tenants can browse without login.
        [AllowAnonymous]
        public async Task<IActionResult> Index(string? searchString, PropertyStatus? status, PropertyType? type)
        {
            var properties = _context.Properties.Include(p => p.Units).AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
                properties = properties.Where(p =>
                    p.Address.Contains(searchString) ||
                    p.City.Contains(searchString) ||
                    (p.Description != null && p.Description.Contains(searchString)));

            if (status.HasValue)
                properties = properties.Where(p => p.Status == status);

            if (type.HasValue)
                properties = properties.Where(p => p.PropertyType == type);

            ViewBag.SearchString = searchString;
            ViewBag.Status = status;
            ViewBag.Type = type;

            // P3: paginate.
            return View(await properties.OrderBy(p => p.Address).ToListAsync());
        }

        // GET: Properties/Details/5  — B10: eager-load Units + recent lease/maintenance
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var property = await _context.Properties
                .Include(p => p.Units)
                .Include(p => p.Leases!).ThenInclude(l => l.Tenant)
                .Include(p => p.MaintenanceRequests)
                .FirstOrDefaultAsync(m => m.PropertyId == id);

            if (property == null) return NotFound();

            return View(property);
        }

        // GET: Properties/Create
        [Authorize(Roles = "PropertyManager")]
        public IActionResult Create() => View(new PropertyCreateViewModel { Address = "", City = "" });

        // POST: Properties/Create — Q4: bind a ViewModel, not the entity.
        [Authorize(Roles = "PropertyManager")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PropertyCreateViewModel vm)
        {
            if (ModelState.IsValid)
            {
                _context.Add(vm.ToEntity());
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Property created successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(vm);
        }

        // GET: Properties/Edit/5
        [Authorize(Roles = "PropertyManager")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var property = await _context.Properties.FindAsync(id);
            if (property == null) return NotFound();
            return View(PropertyCreateViewModel.FromEntity(property));
        }

        // POST: Properties/Edit/5 — Q4: ViewModel-bound.
        [Authorize(Roles = "PropertyManager")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, PropertyCreateViewModel vm)
        {
            if (id != vm.PropertyId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(vm.ToEntity());
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Property updated successfully!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PropertyExists(vm.PropertyId)) return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(vm);
        }

        // GET: Properties/Delete/5
        [Authorize(Roles = "PropertyManager")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var property = await _context.Properties
                .FirstOrDefaultAsync(m => m.PropertyId == id);

            if (property == null) return NotFound();

            return View(property);
        }

        // POST: Properties/Delete/5
        [Authorize(Roles = "PropertyManager")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var property = await _context.Properties.FindAsync(id);
            if (property == null)
            {
                TempData["ErrorMessage"] = "Property not found.";
                return RedirectToAction(nameof(Index));
            }

            // L5: Lease → Property is configured with DeleteBehavior.Restrict, so the SaveChanges
            // call would throw a DbUpdateException for a property that has any lease history.
            // Fail clearly instead of bubbling a 500.
            var blockingLeases = await _context.Leases.AnyAsync(l => l.PropertyId == id);
            if (blockingLeases)
            {
                TempData["ErrorMessage"] = "Cannot delete this property — it still has lease records. Delete or archive the leases first.";
                return RedirectToAction(nameof(Index));
            }

            var blockingMaintenance = await _context.MaintenanceRequests.AnyAsync(m => m.PropertyId == id);
            if (blockingMaintenance)
            {
                TempData["ErrorMessage"] = "Cannot delete this property — it still has maintenance request history.";
                return RedirectToAction(nameof(Index));
            }

            _context.Properties.Remove(property);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Property deleted successfully!";
            return RedirectToAction(nameof(Index));
        }

        private bool PropertyExists(int id)
        {
            return _context.Properties.Any(e => e.PropertyId == id);
        }
    }
}