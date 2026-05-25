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
    public class TenantsController : Controller
    {
        private readonly AppDbContext _context;

        public TenantsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Tenants
        public async Task<IActionResult> Index(int page = 1)
        {
            return View(await _context.Tenants.OrderBy(t => t.FullName).ToListAsync());
        }

        // GET: Tenants/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tenant = await _context.Tenants
                .FirstOrDefaultAsync(m => m.TenantId == id);
            if (tenant == null)
            {
                return NotFound();
            }

            return View(tenant);
        }

        // GET: Tenants/Create
        [Authorize(Roles = "PropertyManager")]
        public IActionResult Create()
            => View(new TenantCreateViewModel { FullName = "", Email = "", Phone = "", NationalId = "" });

        // POST: Tenants/Create — Q4: VM-bound; L13: phone normalised inside ToEntity.
        [HttpPost]
        [Authorize(Roles = "PropertyManager")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TenantCreateViewModel vm)
        {
            if (ModelState.IsValid)
            {
                _context.Add(vm.ToEntity());
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Tenant created successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(vm);
        }

        // GET: Tenants/Edit/5
        [Authorize(Roles = "PropertyManager")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var tenant = await _context.Tenants.FindAsync(id);
            if (tenant == null) return NotFound();
            return View(TenantCreateViewModel.FromEntity(tenant));
        }

        // POST: Tenants/Edit/5 — Q4: VM-bound.
        [HttpPost]
        [Authorize(Roles = "PropertyManager")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, TenantCreateViewModel vm)
        {
            if (id != vm.TenantId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(vm.ToEntity());
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Tenant updated successfully!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TenantExists(vm.TenantId)) return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(vm);
        }

        // GET: Tenants/Delete/5
        [Authorize(Roles = "PropertyManager")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tenant = await _context.Tenants
                .FirstOrDefaultAsync(m => m.TenantId == id);
            if (tenant == null)
            {
                return NotFound();
            }

            return View(tenant);
        }

        // POST: Tenants/Delete/5
        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "PropertyManager")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var tenant = await _context.Tenants.FindAsync(id);
            if (tenant != null)
            {
                _context.Tenants.Remove(tenant);
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Tenant deleted successfully!";
            return RedirectToAction(nameof(Index));
        }

        private bool TenantExists(int id)
        {
            return _context.Tenants.Any(e => e.TenantId == id);
        }
    }
}
