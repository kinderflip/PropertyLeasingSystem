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
    public class LeasesController : Controller
    {
        private readonly AppDbContext _context;

        public LeasesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Leases
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.Leases.Include(l => l.Property).Include(l => l.Tenant);
            return View(await appDbContext.ToListAsync());
        }

        // GET: Leases/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var lease = await _context.Leases
                .Include(l => l.Property)
                .Include(l => l.Tenant)
                .FirstOrDefaultAsync(m => m.LeaseId == id);
            if (lease == null)
            {
                return NotFound();
            }

            return View(lease);
        }

        // GET: Leases/Create
        public IActionResult Create()
        {
            ViewData["PropertyId"] = new SelectList(_context.Properties, "PropertyId", "Address");
            ViewData["TenantId"] = new SelectList(_context.Tenants, "TenantId", "Email");
            return View();
        }

        // POST: Leases/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("LeaseId,PropertyId,TenantId,StartDate,EndDate,MonthlyRent,Status")] Lease lease)
        {
            if (ModelState.IsValid)
            {
                _context.Add(lease);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["PropertyId"] = new SelectList(_context.Properties, "PropertyId", "Address", lease.PropertyId);
            ViewData["TenantId"] = new SelectList(_context.Tenants, "TenantId", "Email", lease.TenantId);
            return View(lease);
        }

        // GET: Leases/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var lease = await _context.Leases.FindAsync(id);
            if (lease == null)
            {
                return NotFound();
            }
            ViewData["PropertyId"] = new SelectList(_context.Properties, "PropertyId", "Address", lease.PropertyId);
            ViewData["TenantId"] = new SelectList(_context.Tenants, "TenantId", "Email", lease.TenantId);
            return View(lease);
        }

        // POST: Leases/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("LeaseId,PropertyId,TenantId,StartDate,EndDate,MonthlyRent,Status")] Lease lease)
        {
            if (id != lease.LeaseId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(lease);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!LeaseExists(lease.LeaseId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["PropertyId"] = new SelectList(_context.Properties, "PropertyId", "Address", lease.PropertyId);
            ViewData["TenantId"] = new SelectList(_context.Tenants, "TenantId", "Email", lease.TenantId);
            return View(lease);
        }

        // GET: Leases/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var lease = await _context.Leases
                .Include(l => l.Property)
                .Include(l => l.Tenant)
                .FirstOrDefaultAsync(m => m.LeaseId == id);
            if (lease == null)
            {
                return NotFound();
            }

            return View(lease);
        }

        // POST: Leases/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var lease = await _context.Leases.FindAsync(id);
            if (lease != null)
            {
                _context.Leases.Remove(lease);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool LeaseExists(int id)
        {
            return _context.Leases.Any(e => e.LeaseId == id);
        }
    }
}
