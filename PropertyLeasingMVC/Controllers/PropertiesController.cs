using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PropertyLeasingAPI.Data;
using PropertyLeasingAPI.Models;
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

        // GET: Properties
        public async Task<IActionResult> Index()
        {
            return View(await _context.Properties.ToListAsync());
        }

        // GET: Properties/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var property = await _context.Properties
                .FirstOrDefaultAsync(m => m.PropertyId == id);

            if (property == null) return NotFound();

            return View(property);
        }

        // GET: Properties/Create
        [Authorize(Roles = "PropertyManager")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Properties/Create
        [Authorize(Roles = "PropertyManager")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("PropertyId,Address,City,PropertyType,Bedrooms,MonthlyRent,Status,Description")] Property property)
        {
            if (ModelState.IsValid)
            {
                _context.Add(property);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Property created successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(property);
        }

        // GET: Properties/Edit/5
        [Authorize(Roles = "PropertyManager")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var property = await _context.Properties.FindAsync(id);
            if (property == null) return NotFound();

            return View(property);
        }

        // POST: Properties/Edit/5
        [Authorize(Roles = "PropertyManager")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("PropertyId,Address,City,PropertyType,Bedrooms,MonthlyRent,Status,Description")] Property property)
        {
            if (id != property.PropertyId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(property);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Property updated successfully!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PropertyExists(property.PropertyId))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(property);
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
            if (property != null)
            {
                _context.Properties.Remove(property);
            }

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