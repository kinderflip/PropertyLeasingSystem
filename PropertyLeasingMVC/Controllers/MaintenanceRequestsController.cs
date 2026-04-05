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
    public class MaintenanceRequestsController : Controller
    {
        private readonly AppDbContext _context;

        public MaintenanceRequestsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: MaintenanceRequests
        public async Task<IActionResult> Index()
        {
            var requests = _context.MaintenanceRequests
                .Include(m => m.Property)
                .Include(m => m.Tenant);
            return View(await requests.ToListAsync());
        }

        // GET: MaintenanceRequests/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var maintenanceRequest = await _context.MaintenanceRequests
                .Include(m => m.Property)
                .Include(m => m.Tenant)
                .FirstOrDefaultAsync(m => m.RequestId == id);

            if (maintenanceRequest == null) return NotFound();

            return View(maintenanceRequest);
        }

        // GET: MaintenanceRequests/Create
        [Authorize(Roles = "PropertyManager,Tenant")]
        public IActionResult Create()
        {
            ViewData["PropertyId"] = new SelectList(_context.Properties, "PropertyId", "Address");
            ViewData["TenantId"] = new SelectList(_context.Tenants, "TenantId", "FullName");
            return View();
        }

        // POST: MaintenanceRequests/Create
        [Authorize(Roles = "PropertyManager,Tenant")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("RequestId,PropertyId,TenantId,Title,Description,Category,Status,DateSubmitted,DateResolved")] MaintenanceRequest maintenanceRequest)
        {
            if (ModelState.IsValid)
            {
                maintenanceRequest.DateSubmitted = DateTime.Now;
                maintenanceRequest.Status = MaintenanceStatus.Pending;
                _context.Add(maintenanceRequest);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Maintenance request submitted successfully!";
                return RedirectToAction(nameof(Index));
            }
            ViewData["PropertyId"] = new SelectList(_context.Properties, "PropertyId", "Address", maintenanceRequest.PropertyId);
            ViewData["TenantId"] = new SelectList(_context.Tenants, "TenantId", "FullName", maintenanceRequest.TenantId);
            return View(maintenanceRequest);
        }

        // GET: MaintenanceRequests/Edit/5
        [Authorize(Roles = "PropertyManager,MaintenanceStaff")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var maintenanceRequest = await _context.MaintenanceRequests.FindAsync(id);
            if (maintenanceRequest == null) return NotFound();

            ViewData["PropertyId"] = new SelectList(_context.Properties, "PropertyId", "Address", maintenanceRequest.PropertyId);
            ViewData["TenantId"] = new SelectList(_context.Tenants, "TenantId", "FullName", maintenanceRequest.TenantId);
            return View(maintenanceRequest);
        }

        // POST: MaintenanceRequests/Edit/5
        [Authorize(Roles = "PropertyManager,MaintenanceStaff")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("RequestId,PropertyId,TenantId,Title,Description,Category,Status,DateSubmitted,DateResolved")] MaintenanceRequest maintenanceRequest)
        {
            if (id != maintenanceRequest.RequestId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    // Auto set resolved date when completed
                    if (maintenanceRequest.Status == MaintenanceStatus.Completed
                        && maintenanceRequest.DateResolved == null)
                    {
                        maintenanceRequest.DateResolved = DateTime.Now;
                    }

                    _context.Update(maintenanceRequest);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Maintenance request updated successfully!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MaintenanceRequestExists(maintenanceRequest.RequestId))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["PropertyId"] = new SelectList(_context.Properties, "PropertyId", "Address", maintenanceRequest.PropertyId);
            ViewData["TenantId"] = new SelectList(_context.Tenants, "TenantId", "FullName", maintenanceRequest.TenantId);
            return View(maintenanceRequest);
        }

        // GET: MaintenanceRequests/Delete/5
        [Authorize(Roles = "PropertyManager")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var maintenanceRequest = await _context.MaintenanceRequests
                .Include(m => m.Property)
                .Include(m => m.Tenant)
                .FirstOrDefaultAsync(m => m.RequestId == id);

            if (maintenanceRequest == null) return NotFound();

            return View(maintenanceRequest);
        }

        // POST: MaintenanceRequests/Delete/5
        [Authorize(Roles = "PropertyManager")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var maintenanceRequest = await _context.MaintenanceRequests.FindAsync(id);
            if (maintenanceRequest != null)
            {
                _context.MaintenanceRequests.Remove(maintenanceRequest);
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Maintenance request deleted successfully!";
            return RedirectToAction(nameof(Index));
        }

        private bool MaintenanceRequestExists(int id)
        {
            return _context.MaintenanceRequests.Any(e => e.RequestId == id);
        }
    }
}