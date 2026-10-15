using CollegeManagementSystem.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CollegeManagementSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class NotificationController : ControllerBase
    {
        private readonly AppDbContext _context;

        public NotificationController(AppDbContext context)
        {
            _context = context;
        }

        private int GetUserId()
        {
            return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        }

        // GET: api/Notification/my
        [HttpGet("my")]
        public async Task<IActionResult> GetMy()
        {
            var userId = GetUserId();

            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .Select(n => new
                {
                    n.NotificationId,
                    n.Title,
                    n.Message,
                    n.Type,
                    n.IsRead,
                    n.CreatedAt
                })
                .ToListAsync();

            return Ok(notifications);
        }

        // PUT: api/Notification/read/{id}
        [HttpPut("read/{id}")]
        public async Task<IActionResult> MarkRead(int id)
        {
            var userId = GetUserId();

            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.NotificationId == id && n.UserId == userId);

            if (notification == null)
                return NotFound(new { message = "Notification maldo nahi." });

            notification.IsRead = true;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Notification read thai gayu." });
        }

        // PUT: api/Notification/read-all
        [HttpPut("read-all")]
        public async Task<IActionResult> MarkAllRead()
        {
            var userId = GetUserId();

            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToListAsync();

            foreach (var n in notifications)
                n.IsRead = true;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Badha notifications read thai gaya.", count = notifications.Count });
        }
    }
}