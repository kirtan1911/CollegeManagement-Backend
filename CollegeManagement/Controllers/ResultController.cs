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
    public class ResultController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ResultController(AppDbContext context)
        {
            _context = context;
        }

        private int GetUserId() =>
            int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        // ── Helper: look up grade from DB GradeScale ──────────────────────────
        private async Task<(string Grade, decimal GradePoint)> GetGradeAsync(decimal totalMarks)
        {
            var scale = await _context.GradeScales
                .Where(g => totalMarks >= g.MinMarks && totalMarks <= g.MaxMarks)
                .FirstOrDefaultAsync();

            if (scale == null) return ("Fail", 0);
            return (scale.Grade, scale.GradePoint);
        }

        // ── GET: api/Result/student/{studentProfileId} ── Admin/Staff/Faculty
        [Authorize(Roles = "Admin,Staff,Faculty")]
        [HttpGet("student/{studentProfileId}")]
        public async Task<IActionResult> GetStudentResult(int studentProfileId)
        {
            return await BuildResultResponse(studentProfileId);
        }

        // ── GET: api/Result/my ── Student (self)
        [Authorize(Roles = "Student")]
        [HttpGet("my")]
        public async Task<IActionResult> GetMyResult()
        {
            var userId = GetUserId();
            var profile = await _context.StudentProfiles
                .FirstOrDefaultAsync(sp => sp.UserId == userId);

            if (profile == null)
                return NotFound(new { message = "Profile maldo nahi." });

            return await BuildResultResponse(profile.StudentProfileId);
        }

        private async Task<IActionResult> BuildResultResponse(int studentProfileId)
        {
            // Fetch internal marks (with course + credits)
            var internals = await _context.InternalMarks
                .Include(im => im.Course)
                .Where(im => im.StudentProfileId == studentProfileId)
                .ToListAsync();

            // Fetch external marks
            var externals = await _context.ExternalMarks
                .Include(em => em.Course)
                .Where(em => em.StudentProfileId == studentProfileId)
                .ToListAsync();

            // Merge by CourseId
            var courseIds = internals.Select(i => i.CourseId)
                .Union(externals.Select(e => e.CourseId))
                .Distinct()
                .ToList();

            var subjectResults = new List<object>();

            foreach (var courseId in courseIds)
            {
                var im = internals.FirstOrDefault(i => i.CourseId == courseId);
                var em = externals.FirstOrDefault(e => e.CourseId == courseId);

                var totalInternal = im?.TotalInternal ?? 0m;
                var externalScore = em?.ExternalScore ?? 0m;
                var total = totalInternal + externalScore;           // max 100
                var credits = im?.Course?.Credits ?? em?.Course?.Credits ?? 0;
                var courseName = im?.Course?.CourseName ?? em?.Course?.CourseName ?? "Unknown";
                var courseCode = im?.Course?.CourseCode ?? em?.Course?.CourseCode ?? "-";
                var semester = im?.Course?.Semester ?? em?.Course?.Semester ?? 0;

                var isPassing = total >= 40;
                var (grade, gradePoint) = await GetGradeAsync(total);

                subjectResults.Add(new
                {
                    CourseId = courseId,
                    CourseName = courseName,
                    CourseCode = courseCode,
                    Semester = semester,
                    Credits = credits,
                    TotalInternal = totalInternal,
                    ExternalScore = externalScore,
                    Total = total,
                    Grade = grade,
                    GradePoint = gradePoint,
                    IsPassing = isPassing,
                    Status = isPassing ? "Pass" : "Fail"
                });
            }

            // Group by semester for SPI
            var bySemester = subjectResults
                .Cast<dynamic>()
                .GroupBy(s => (int)s.Semester)
                .Select(semGroup =>
                {
                    bool hasAtkt = semGroup.Any(s => !(bool)s.IsPassing);
                    decimal totalCredits = semGroup.Sum(s => (int)s.Credits);
                    decimal weightedPoints = semGroup.Sum(s => (int)s.Credits * (decimal)s.GradePoint);
                    decimal spi = totalCredits > 0 ? Math.Round(weightedPoints / totalCredits, 2) : 0;

                    return new
                    {
                        Semester = semGroup.Key,
                        SPI = spi,
                        Status = hasAtkt ? "Fail — ATKT" : "Pass",
                        Subjects = semGroup.ToList()
                    };
                })
                .OrderBy(s => s.Semester)
                .ToList();

            // CGPA = credit-weighted average of all completed subjects
            var allCredits = (decimal)subjectResults.Cast<dynamic>().Sum(s => (int)s.Credits);
            var allWeighted = (decimal)subjectResults.Cast<dynamic>()
                .Sum(s => (int)s.Credits * (decimal)s.GradePoint);
            var cgpa = allCredits > 0 ? Math.Round(allWeighted / allCredits, 2) : 0m;

            return Ok(new
            {
                StudentProfileId = studentProfileId,
                CGPA = cgpa,
                Semesters = bySemester
            });
        }
    }
}
