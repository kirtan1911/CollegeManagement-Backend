using CollegeManagementSystem.Data;
using CollegeManagementSystem.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CollegeManagementSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]   // Phase 11: was entirely missing [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UsersController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Users
        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _context.Users
                .Include(x => x.Role)
                .Select(x => new
                {
                    x.UserId,
                    x.Name,
                    x.Email,
                    Role = x.Role != null ? x.Role.RoleName : "Unknown"
                })
                .ToListAsync();

            return Ok(users);
        }

        // GET: api/Users/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUser(int id)
        {
            var user = await _context.Users
                .Include(x => x.Role)
                .Where(x => x.UserId == id)
                .Select(x => new
                {
                    x.UserId,
                    x.Name,
                    x.Email,
                    Role = x.Role != null ? x.Role.RoleName : "Unknown"
                })
                .FirstOrDefaultAsync();

            if (user == null)
                return NotFound(new { message = "User maldo nahi." });

            return Ok(user);
        }

        // PUT: api/Users/5  — Phase 11: uses UpdateUserDTO instead of raw User model
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserDTO dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.UserId == id);
            if (user == null)
                return NotFound(new { message = "User maldo nahi." });

            user.Name = dto.Name;
            user.Email = dto.Email;

            // Look up RoleId from name if provided
            if (!string.IsNullOrWhiteSpace(dto.RoleName))
            {
                var role = await _context.Roles
                    .FirstOrDefaultAsync(r => r.RoleName == dto.RoleName);
                if (role == null)
                    return BadRequest(new { message = "Role maldo nahi." });
                user.RoleId = role.RoleId;
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "User update thayo!" });
        }

        // DELETE: api/Users/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.UserId == id);
            if (user == null)
                return NotFound(new { message = "User maldo nahi." });

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return Ok(new { message = "User delete thayo." });
        }
    }
}