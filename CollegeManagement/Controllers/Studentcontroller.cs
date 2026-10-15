using CollegeManagementSystem.Data;
using CollegeManagementSystem.DTOs;
using CollegeManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CollegeManagementSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Student")]
    public class StudentController : ControllerBase
    {
        private readonly AppDbContext _context;

        public StudentController(AppDbContext context)
        {
            _context = context;
        }

        // Helper: JWT token ma thi current logged in UserId kado
        private int GetUserId()
        {
            return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        }

        // GET: api/Student/profile
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var userId = GetUserId();

            var profile = await _context.StudentProfiles
                .Include(x => x.User)
                .Include(x => x.Department)
                .FirstOrDefaultAsync(x => x.UserId == userId);

            if (profile == null)
                return NotFound("Profile not found");

            return Ok(new
            {
                profile.StudentProfileId,
                profile.EnrollmentNo,
                profile.Semester,
                Name = profile.User!.Name,
                Email = profile.User.Email,
                Department = profile.Department!.DepartmentName,
                // Phase 7 — Commute Details
                profile.ResidenceType,
                profile.HomeTown,
                profile.TransportOpted,
                profile.TransportRoute
            });
        }

        // GET: api/Student/courses
        [HttpGet("courses")]
        public async Task<IActionResult> GetCourses()
        {
            var userId = GetUserId();

            var profile = await _context.StudentProfiles
                .FirstOrDefaultAsync(x => x.UserId == userId);

            if (profile == null)
                return NotFound("Profile not found");

            var courses = await _context.Enrollments
                .Include(x => x.Course)
                    .ThenInclude(c => c!.Department)
                .Where(x => x.StudentProfileId == profile.StudentProfileId)
                .Select(x => new
                {
                    x.EnrollmentId,
                    x.CourseId,
                    x.Course!.CourseCode,
                    x.Course.CourseName,
                    x.Course.Semester,
                    x.Course.Credits,
                    Department = x.Course.Department!.DepartmentName,
                    x.EnrolledDate
                })
                .OrderBy(x => x.Semester)
                .ThenBy(x => x.CourseName)
                .ToListAsync();

            return Ok(courses);
        }

        // GET: api/Student/attendance
        [HttpGet("attendance")]
        public async Task<IActionResult> GetAttendance()
        {
            var userId = GetUserId();

            var profile = await _context.StudentProfiles
                .FirstOrDefaultAsync(x => x.UserId == userId);

            if (profile == null)
                return NotFound("Profile not found");

            var attendance = await _context.Attendances
                .Include(x => x.Enrollment)
                    .ThenInclude(e => e!.Course)
                .Where(x => x.Enrollment!.StudentProfileId == profile.StudentProfileId)
                .Select(x => new
                {
                    x.AttendanceId,
                    Course = x.Enrollment!.Course!.CourseName,
                    x.Date,
                    x.IsPresent
                })
                .OrderByDescending(x => x.Date)
                .ToListAsync();

            // Summary per course
            var summary = attendance
                .GroupBy(x => x.Course)
                .Select(g => new
                {
                    Course = g.Key,
                    TotalClasses = g.Count(),
                    Present = g.Count(x => x.IsPresent),
                    Absent = g.Count(x => !x.IsPresent),
                    Percentage = Math.Round((double)g.Count(x => x.IsPresent) / g.Count() * 100, 2)
                });

            return Ok(new
            {
                Summary = summary,
                Detail = attendance
            });
        }

        // GET: api/Student/marks
        [HttpGet("marks")]
        public async Task<IActionResult> GetMarks()
        {
            var userId = GetUserId();

            var profile = await _context.StudentProfiles
                .FirstOrDefaultAsync(x => x.UserId == userId);

            if (profile == null)
                return NotFound("Profile not found");

            var marks = await _context.Marks
                .Include(x => x.Enrollment)
                    .ThenInclude(e => e!.Course)
                .Where(x => x.Enrollment!.StudentProfileId == profile.StudentProfileId)
                .Select(x => new
                {
                    x.MarkId,
                    Course = x.Enrollment!.Course!.CourseName,
                    x.ExamType,
                    x.ObtainedMarks,
                    x.TotalMarks,
                    Percentage = Math.Round((double)x.ObtainedMarks / (double)x.TotalMarks * 100, 2)
                })
                .OrderBy(x => x.Course)
                .ToListAsync();

            return Ok(marks);
        }

        // PUT: api/Student/profile — Student updates their own commute / residence details
        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateCommuteDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = GetUserId();

            var profile = await _context.StudentProfiles
                .FirstOrDefaultAsync(x => x.UserId == userId);

            if (profile == null)
                return NotFound(new { message = "Profile not found." });

            // Students may only update their own commute/residence information
            profile.ResidenceType   = dto.ResidenceType;
            profile.HomeTown        = dto.HomeTown?.Trim() ?? string.Empty;
            profile.TransportOpted  = dto.TransportOpted;
            profile.TransportRoute  = dto.TransportOpted ? dto.TransportRoute?.Trim() : null;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Profile updated successfully." });
        }
    }
}