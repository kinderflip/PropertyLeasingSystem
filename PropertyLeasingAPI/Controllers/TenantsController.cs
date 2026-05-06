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
    public class TenantsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TenantsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Tenants — manager-only directory.
        // C3: tenants directory was readable by every authenticated role; lock to PropertyManager.
        [HttpGet]
        [Authorize(Roles = Roles.PropertyManager)]
        public async Task<ActionResult<IEnumerable<Tenant>>> GetTenants()
        {
            return await _context.Tenants.ToListAsync();
        }

        // GET: api/Tenants/5
        // C3: Manager → any; Tenant role → only their own record.
        [HttpGet("{id}")]
        public async Task<ActionResult<Tenant>> GetTenant(int id)
        {
            var tenant = await _context.Tenants.FindAsync(id);
            if (tenant == null) return NotFound();

            if (!User.IsInRole(Roles.PropertyManager))
            {
                var callerUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(callerUserId) || tenant.UserId != callerUserId)
                    return Forbid();
            }

            return tenant;
        }

        // POST: api/Tenants
        [HttpPost]
        [Authorize(Roles = Roles.PropertyManager)]
        public async Task<ActionResult<Tenant>> PostTenant(Tenant tenant)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            _context.Tenants.Add(tenant);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetTenant),
                new { id = tenant.TenantId }, tenant);
        }

        // PUT: api/Tenants/5
        [HttpPut("{id}")]
        [Authorize(Roles = Roles.PropertyManager)]
        public async Task<IActionResult> PutTenant(int id, Tenant tenant)
        {
            if (id != tenant.TenantId) return BadRequest();
            if (!ModelState.IsValid) return BadRequest(ModelState);

            _context.Entry(tenant).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Tenants.AnyAsync(t => t.TenantId == id))
                    return NotFound();
                throw;
            }

            return NoContent();
        }

        // DELETE: api/Tenants/5
        [HttpDelete("{id}")]
        [Authorize(Roles = Roles.PropertyManager)]
        public async Task<IActionResult> DeleteTenant(int id)
        {
            var tenant = await _context.Tenants.FindAsync(id);
            if (tenant == null) return NotFound();

            _context.Tenants.Remove(tenant);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}