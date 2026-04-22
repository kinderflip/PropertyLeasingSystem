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
    public class NotificationsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public NotificationsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Notifications — PropertyManager only (admin view)
        [HttpGet]
        [Authorize(Roles = "PropertyManager")]
        public async Task<ActionResult<IEnumerable<Notification>>> GetNotifications()
        {
            return await _context.Notifications
                .OrderByDescending(n => n.CreatedAt)
                .AsNoTracking()
                .ToListAsync();
        }

        // GET: api/Notifications/user/{userId}
        // B3: callers can only request their own; PropertyManager bypasses the check.
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<Notification>>> GetUserNotifications(string userId)
        {
            if (!IsSelfOrManager(userId)) return Forbid();

            return await _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .AsNoTracking()
                .ToListAsync();
        }

        // GET: api/Notifications/unread/{userId}
        [HttpGet("unread/{userId}")]
        public async Task<ActionResult<int>> GetUnreadCount(string userId)
        {
            if (!IsSelfOrManager(userId)) return Forbid();

            var count = await _context.Notifications
                .CountAsync(n => n.UserId == userId && !n.IsRead);
            return count;
        }

        // PUT: api/Notifications/read/5
        [HttpPut("read/{id}")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var notification = await _context.Notifications.FindAsync(id);
            if (notification == null) return NotFound();

            if (!IsSelfOrManager(notification.UserId)) return Forbid();

            notification.IsRead = true;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // PUT: api/Notifications/readall/{userId}
        [HttpPut("readall/{userId}")]
        public async Task<IActionResult> MarkAllAsRead(string userId)
        {
            if (!IsSelfOrManager(userId)) return Forbid();

            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToListAsync();

            foreach (var n in notifications)
                n.IsRead = true;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // B3: ownership guard — caller must match userId or be a PropertyManager.
        private bool IsSelfOrManager(string targetUserId)
        {
            if (User.IsInRole("PropertyManager")) return true;
            var callerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return !string.IsNullOrEmpty(callerId) && callerId == targetUserId;
        }
    }
}
