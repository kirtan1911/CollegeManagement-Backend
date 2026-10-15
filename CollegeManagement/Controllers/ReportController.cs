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
    public class ReportController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ReportController(AppDbContext context)
        {
            _context = context;
        }

        private int GetUserId() =>
            int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        // ── GET: api/Report/my ── Student's own report (IDOR-safe)
        [Authorize(Roles = "Student")]
        [HttpGet("my")]
        public async Task<IActionResult> GetMyReport()
        {
            var userId = GetUserId();
            var profile = await _context.StudentProfiles
                .FirstOrDefaultAsync(sp => sp.UserId == userId);

            if (profile == null)
                return NotFound(new { message = "Student profile not found." });

            return await BuildStudentReport(profile.StudentProfileId);
        }

        // ── GET: api/Report/student/{studentProfileId} ── Admin/Staff/Faculty only
        [Authorize(Roles = "Admin,Staff,Faculty")]
        [HttpGet("student/{studentProfileId}")]
        public async Task<IActionResult> StudentReport(int studentProfileId)
        {
            return await BuildStudentReport(studentProfileId);
        }

        // ── Shared builder: loads and returns the full student report ──
        private async Task<IActionResult> BuildStudentReport(int studentProfileId)
        {
            var profile = await _context.StudentProfiles
                .Include(sp => sp.User)
                .Include(sp => sp.Department)
                .FirstOrDefaultAsync(sp => sp.StudentProfileId == studentProfileId);

            if (profile == null)
                return NotFound(new { message = "Student profile not found." });

            var internalMarks = await _context.InternalMarks
                .Include(im => im.Course)
                .Where(im => im.StudentProfileId == studentProfileId)
                .Select(im => new
                {
                    Course = im.Course!.CourseName,
                    CourseCode = im.Course.CourseCode,
                    Semester = im.Course.Semester,
                    im.Internal1Score,
                    im.Internal2Score,
                    im.BestInternalScore,
                    im.AttendanceMarks,
                    im.QuizMarks,
                    im.FacultyDiscretionMarks,
                    im.TotalInternal
                })
                .ToListAsync();

            var externalMarks = await _context.ExternalMarks
                .Include(em => em.Course)
                .Where(em => em.StudentProfileId == studentProfileId)
                .Select(em => new
                {
                    Course = em.Course!.CourseName,
                    CourseCode = em.Course.CourseCode,
                    em.ExternalScore
                })
                .ToListAsync();

            var attendanceSummary = await _context.Attendances
                .Include(a => a.Enrollment).ThenInclude(e => e!.Course)
                .Where(a => a.Enrollment!.StudentProfileId == studentProfileId)
                .GroupBy(a => a.Enrollment!.Course!.CourseName)
                .Select(g => new
                {
                    Course = g.Key,
                    Total = g.Count(),
                    Present = g.Count(x => x.IsPresent),
                    Percentage = g.Count() > 0
                        ? Math.Round((double)g.Count(x => x.IsPresent) / g.Count() * 100, 2)
                        : 0.0
                })
                .ToListAsync();

            var fees = await _context.FeeRecords
                .Where(f => f.StudentProfileId == studentProfileId)
                .Select(f => new
                {
                    f.FeeId,
                    f.Amount,
                    f.DueDate,
                    f.IsPaid,
                    f.ReceiptNumber,
                    f.Description
                })
                .ToListAsync();

            return Ok(new
            {
                Student = new
                {
                    profile.StudentProfileId,
                    profile.EnrollmentNo,
                    profile.Semester,
                    Name = profile.User!.Name,
                    Email = profile.User.Email,
                    Department = profile.Department!.DepartmentName
                },
                InternalMarks = internalMarks,
                ExternalMarks = externalMarks,
                AttendanceSummary = attendanceSummary,
                Fees = fees
            });
        }

        // GET: api/Report/course/{courseId}
        // All students' consolidated marks for a course — Admin/Staff use
        [Authorize(Roles = "Admin,Staff,Faculty")]
        [HttpGet("course/{courseId}")]
        public async Task<IActionResult> CourseReport(int courseId)
        {
            var course = await _context.Courses
                .Include(c => c.Department)
                .FirstOrDefaultAsync(c => c.CourseId == courseId);

            if (course == null)
                return NotFound(new { message = "Course not found." });

            var students = await _context.Enrollments
                .Include(e => e.StudentProfile).ThenInclude(sp => sp!.User)
                .Where(e => e.CourseId == courseId)
                .Select(e => new
                {
                    e.StudentProfile!.StudentProfileId,
                    e.StudentProfile.EnrollmentNo,
                    Name = e.StudentProfile.User!.Name,

                    Internal = _context.InternalMarks
                        .Where(im => im.StudentProfileId == e.StudentProfile.StudentProfileId
                                  && im.CourseId == courseId)
                        .Select(im => new { im.TotalInternal, im.BestInternalScore })
                        .FirstOrDefault(),

                    External = _context.ExternalMarks
                        .Where(em => em.StudentProfileId == e.StudentProfile.StudentProfileId
                                  && em.CourseId == courseId)
                        .Select(em => new { em.ExternalScore })
                        .FirstOrDefault(),

                    Attendance = _context.Attendances
                        .Where(a => a.Enrollment!.StudentProfileId == e.StudentProfile.StudentProfileId
                                 && a.Enrollment.CourseId == courseId)
                        .GroupBy(a => 1)
                        .Select(g => new
                        {
                            Total = g.Count(),
                            Present = g.Count(x => x.IsPresent),
                            Percentage = g.Count() > 0
                                ? Math.Round(
                                    (double)g.Count(x => x.IsPresent) / g.Count() * 100, 2)
                                : 0.0
                        })
                        .FirstOrDefault()
                })
                .ToListAsync();

            return Ok(new
            {
                Course = course.CourseName,
                CourseCode = course.CourseCode,
                Department = course.Department!.DepartmentName,
                Students = students
            });
        }

        // GET: api/Report/fee-summary
        // All unpaid fees — Admin only
        [Authorize(Roles = "Admin")]
        [HttpGet("fee-summary")]
        public async Task<IActionResult> FeeSummary()
        {
            var unpaid = await _context.FeeRecords
                .Include(f => f.StudentProfile).ThenInclude(sp => sp!.User)
                .Where(f => !f.IsPaid)
                .OrderBy(f => f.DueDate)
                .Select(f => new
                {
                    f.FeeId,
                    StudentName = f.StudentProfile!.User!.Name,
                    f.StudentProfile.EnrollmentNo,
                    f.Amount,
                    f.DueDate,
                    f.Description,
                    Overdue = f.DueDate < DateTime.UtcNow
                })
                .ToListAsync();

            var totalPaid = await _context.FeeRecords
                .Where(f => f.IsPaid)
                .SumAsync(f => f.Amount);

            var totalUnpaid = await _context.FeeRecords
                .Where(f => !f.IsPaid)
                .SumAsync(f => f.Amount);

            return Ok(new
            {
                TotalPaid = totalPaid,
                TotalUnpaid = totalUnpaid,
                UnpaidRecords = unpaid
            });
        }
    }
}