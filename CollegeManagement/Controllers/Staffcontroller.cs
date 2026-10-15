using CollegeManagementSystem.Data;
using CollegeManagementSystem.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CollegeManagementSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Staff,Faculty")]
    public class StaffController : ControllerBase
    {
        private readonly AppDbContext _context;

        public StaffController(AppDbContext context)
        {
            _context = context;
        }

        // Helper: JWT token ma thi current logged in UserId kado
        private int GetUserId()
        {
            return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        }

        // GET: api/Staff/courses?departmentId=1&semester=5
        // Badha courses jova (optional filters)
        [HttpGet("courses")]
        public async Task<IActionResult> GetCourses([FromQuery] int? departmentId, [FromQuery] int? semester)
        {
            var query = _context.Courses
                .Include(x => x.Department)
                .AsQueryable();

            if (departmentId.HasValue)
                query = query.Where(c => c.DepartmentId == departmentId.Value);

            if (semester.HasValue)
                query = query.Where(c => c.Semester == semester.Value);

            var courses = await query
                .Select(x => new
                {
                    x.CourseId,
                    x.CourseCode,
                    x.CourseName,
                    x.Semester,
                    Department = x.Department!.DepartmentName,
                    x.DepartmentId
                })
                .OrderBy(x => x.Semester)
                .ThenBy(x => x.CourseName)
                .ToListAsync();

            return Ok(courses);
        }

        // GET: api/Staff/course/{courseId}/students
        // Ek course na badha students jova
        [HttpGet("course/{courseId}/students")]
        public async Task<IActionResult> GetStudentsByCourse(int courseId)
        {
            var students = await _context.Enrollments
                .Include(x => x.StudentProfile)
                    .ThenInclude(sp => sp!.User)
                .Where(x => x.CourseId == courseId)
                .Select(x => new
                {
                    x.EnrollmentId,
                    x.StudentProfile!.EnrollmentNo,
                    Name = x.StudentProfile.User!.Name,
                    Email = x.StudentProfile.User.Email,
                    x.StudentProfile.Semester
                })
                .ToListAsync();

            return Ok(students);
        }

        // POST: api/Staff/attendance
        // Student ni attendance mark karvi
        [HttpPost("attendance")]
        public async Task<IActionResult> MarkAttendance(AttendanceDTO dto)
        {
            var staffId = GetUserId();

            // Attendance date cannot be in the future
            if (dto.Date.Date > DateTime.UtcNow.Date)
                return BadRequest(new { message = "Attendance cannot be marked for a future date." });

            // Enrollment exist kare che ke nahi check karo
            var enrollment = await _context.Enrollments
                .FirstOrDefaultAsync(x => x.EnrollmentId == dto.EnrollmentId);

            if (enrollment == null)
                return NotFound("Enrollment not found");

            // Same date + enrollment ni attendance already che ke nahi
            var existing = await _context.Attendances
                .FirstOrDefaultAsync(x =>
                    x.EnrollmentId == dto.EnrollmentId &&
                    x.Date.Date == dto.Date.Date);

            if (existing != null)
            {
                // Already che to update karo
                existing.IsPresent = dto.IsPresent;
                await _context.SaveChangesAsync();
                return Ok("Attendance Updated Successfully");
            }

            // Navi attendance add karo
            var attendance = new CollegeManagementSystem.Models.Attendance
            {
                EnrollmentId = dto.EnrollmentId,
                Date = dto.Date,
                IsPresent = dto.IsPresent,
                MarkedByUserId = staffId
            };

            _context.Attendances.Add(attendance);
            await _context.SaveChangesAsync();

            return Ok("Attendance Marked Successfully");
        }

        // POST: api/Staff/marks
        // Student na marks enter karva
        [HttpPost("marks")]
        public async Task<IActionResult> AddMarks(MarkDTO dto)
        {
            var staffId = GetUserId();

            if (dto.TotalMarks <= 0)
                return BadRequest(new { message = "TotalMarks must be greater than 0." });

            if (dto.ObtainedMarks < 0)
                return BadRequest(new { message = "ObtainedMarks cannot be negative." });

            if (dto.ObtainedMarks > dto.TotalMarks)
                return BadRequest(new { message = "Obtained marks cannot exceed total marks." });

            // Enrollment exist kare che ke nahi check karo
            var enrollment = await _context.Enrollments
                .FirstOrDefaultAsync(x => x.EnrollmentId == dto.EnrollmentId);

            if (enrollment == null)
                return NotFound("Enrollment not found");

            // Same enrollment + exam type already che ke nahi
            var existing = await _context.Marks
                .FirstOrDefaultAsync(x =>
                    x.EnrollmentId == dto.EnrollmentId &&
                    x.ExamType == dto.ExamType);

            if (existing != null)
            {
                // Already che to update karo
                existing.ObtainedMarks = dto.ObtainedMarks;
                existing.TotalMarks = dto.TotalMarks;
                await _context.SaveChangesAsync();
                return Ok("Marks Updated Successfully");
            }

            // Nava marks add karo
            var mark = new CollegeManagementSystem.Models.Mark
            {
                EnrollmentId = dto.EnrollmentId,
                ExamType = dto.ExamType,
                ObtainedMarks = dto.ObtainedMarks,
                TotalMarks = dto.TotalMarks,
                AddedByUserId = staffId
            };

            _context.Marks.Add(mark);
            await _context.SaveChangesAsync();

            return Ok("Marks Added Successfully");
        }

        // GET: api/Staff/attendance/{courseId}
        // Ek course ni badhi attendance jovi
        [HttpGet("attendance/{courseId}")]
        public async Task<IActionResult> GetAttendance(int courseId)
        {
            var attendance = await _context.Attendances
                .Include(x => x.Enrollment)
                    .ThenInclude(e => e!.StudentProfile)
                        .ThenInclude(sp => sp!.User)
                .Include(x => x.Enrollment)
                    .ThenInclude(e => e!.Course)
                .Where(x => x.Enrollment!.CourseId == courseId)
                .Select(x => new
                {
                    x.AttendanceId,
                    Name = x.Enrollment!.StudentProfile!.User!.Name,
                    x.Enrollment.StudentProfile.EnrollmentNo,
                    Course = x.Enrollment.Course!.CourseName,
                    x.Date,
                    x.IsPresent
                })
                .OrderByDescending(x => x.Date)
                .ToListAsync();

            return Ok(attendance);
        }

        // GET: api/Staff/marks/{courseId}
        // Ek course na badha marks jova
        [HttpGet("marks/{courseId}")]
        public async Task<IActionResult> GetMarks(int courseId)
        {
            var marks = await _context.Marks
                .Include(x => x.Enrollment)
                    .ThenInclude(e => e!.StudentProfile)
                        .ThenInclude(sp => sp!.User)
                .Include(x => x.Enrollment)
                    .ThenInclude(e => e!.Course)
                .Where(x => x.Enrollment!.CourseId == courseId)
                .Select(x => new
                {
                    x.MarkId,
                    Name = x.Enrollment!.StudentProfile!.User!.Name,
                    x.Enrollment.StudentProfile.EnrollmentNo,
                    Course = x.Enrollment.Course!.CourseName,
                    x.ExamType,
                    x.ObtainedMarks,
                    x.TotalMarks,
                    Percentage = x.TotalMarks > 0
                        ? Math.Round((double)x.ObtainedMarks / (double)x.TotalMarks * 100, 2)
                        : 0.0
                })
                .OrderBy(x => x.Name)
                .ToListAsync();

            return Ok(marks);
        }
    }
}