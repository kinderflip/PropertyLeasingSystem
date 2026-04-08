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
    public class PropertiesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public PropertiesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Properties
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Property>>> GetProperties()
        {
            return await _context.Properties.ToListAsync();
        }

        // GET: api/Properties/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Property>> GetProperty(int id)
        {
            var property = await _context.Properties.FindAsync(id);
            if (property == null) return NotFound();
            return property;
        }

        // GET: api/Properties/available
        [HttpGet("available")]
        public async Task<ActionResult<IEnumerable<Property>>> GetAvailableProperties()
        {
            return await _context.Properties
                .Where(p => p.Status == PropertyStatus.Available)
                .ToListAsync();
        }

        // POST: api/Properties
        [HttpPost]
        [Authorize(Roles = "PropertyManager")]
        public async Task<ActionResult<Property>> PostProperty(Property property)
        {
            _context.Properties.Add(property);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetProperty),
                new { id = property.PropertyId }, property);
        }

        // PUT: api/Properties/5
        [HttpPut("{id}")]
        [Authorize(Roles = "PropertyManager")]
        public async Task<IActionResult> PutProperty(int id, Property property)
        {
            if (id != property.PropertyId) return BadRequest();

            _context.Entry(property).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Properties.AnyAsync(p => p.PropertyId == id))
                    return NotFound();
                throw;
            }

            return NoContent();
        }

        // DELETE: api/Properties/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "PropertyManager")]
        public async Task<IActionResult> DeleteProperty(int id)
        {
            var property = await _context.Properties.FindAsync(id);
            if (property == null) return NotFound();

            _context.Properties.Remove(property);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}