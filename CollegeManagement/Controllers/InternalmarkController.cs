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
    [Authorize]
    public class InternalMarkController : ControllerBase
    {
        private readonly AppDbContext _context;

        public InternalMarkController(AppDbContext context)
        {
            _context = context;
        }

        private int GetUserId()
        {
            return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        }

        // PUT: api/InternalMark/update
        [Authorize(Roles = "Staff,Faculty")]
        [HttpPut("update")]
        public async Task<IActionResult> UpdateInternalMarks(UpdateInternalDTO dto)
        {
            // Validate ranges
            if (dto.AttendanceMarks < 0 || dto.AttendanceMarks > 5)
                return BadRequest(new { message = "AttendanceMarks 0 thi 5 ni vachhe hova joiye." });

            if (dto.QuizMarks < 0 || dto.QuizMarks > 5)
                return BadRequest(new { message = "QuizMarks 0 thi 5 ni vachhe hova joiye." });

            if (dto.FacultyDiscretionMarks < 0 || dto.FacultyDiscretionMarks > 10)
                return BadRequest(new { message = "FacultyDiscretionMarks 0 thi 10 ni vachhe hova joiye." });

            var internalMark = await _context.InternalMarks
                .FirstOrDefaultAsync(im => im.StudentProfileId == dto.StudentProfileId
                                         && im.CourseId == dto.CourseId);

            if (internalMark == null)
            {
                var profileExists = await _context.StudentProfiles
                    .AnyAsync(sp => sp.StudentProfileId == dto.StudentProfileId);
                var courseExists = await _context.Courses
                    .AnyAsync(c => c.CourseId == dto.CourseId);

                if (!profileExists) return NotFound(new { message = "Student profile not found." });
                if (!courseExists) return NotFound(new { message = "Course not found." });

                // Verify the student is actually enrolled in this course
                var isEnrolled = await _context.Enrollments
                    .AnyAsync(e => e.StudentProfileId == dto.StudentProfileId
                                && e.CourseId == dto.CourseId);
                if (!isEnrolled)
                    return BadRequest(new { message = "Student is not enrolled in this course. Cannot enter marks." });

                internalMark = new InternalMark
                {
                    StudentProfileId = dto.StudentProfileId,
                    CourseId = dto.CourseId
                };
                _context.InternalMarks.Add(internalMark);
            }

            internalMark.AttendanceMarks = dto.AttendanceMarks;
            internalMark.QuizMarks = dto.QuizMarks;
            internalMark.FacultyDiscretionMarks = dto.FacultyDiscretionMarks;

            internalMark.BestInternalScore = Math.Max(internalMark.Internal1Score, internalMark.Internal2Score);

            internalMark.TotalInternal =
                internalMark.BestInternalScore +
                internalMark.AttendanceMarks +
                internalMark.QuizMarks +
                internalMark.FacultyDiscretionMarks;

            // Cap at 40
            if (internalMark.TotalInternal > 40)
                internalMark.TotalInternal = 40;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Internal marks update thaya!",
                internalMark.InternalMarkId,
                internalMark.BestInternalScore,
                internalMark.AttendanceMarks,
                internalMark.QuizMarks,
                internalMark.FacultyDiscretionMarks,
                internalMark.TotalInternal
            });
        }

        // GET: api/InternalMark/course/{courseId}
        [Authorize(Roles = "Admin,Staff,Faculty")]
        [HttpGet("course/{courseId}")]
        public async Task<IActionResult> GetByCourse(int courseId)
        {
            var marks = await _context.InternalMarks
                .Include(im => im.StudentProfile)
                    .ThenInclude(sp => sp!.User)
                .Where(im => im.CourseId == courseId)
                .Select(im => new
                {
                    im.InternalMarkId,
                    im.StudentProfileId,
                    StudentName = im.StudentProfile!.User!.Name,
                    EnrollmentNo = im.StudentProfile.EnrollmentNo,
                    im.Internal1Score,
                    im.Internal2Score,
                    im.BestInternalScore,
                    im.AttendanceMarks,
                    im.QuizMarks,
                    im.FacultyDiscretionMarks,
                    im.TotalInternal
                })
                .OrderBy(x => x.StudentName)
                .ToListAsync();

            return Ok(marks);
        }

        // GET: api/InternalMark/my-marks
        [Authorize(Roles = "Student")]
        [HttpGet("my-marks")]
        public async Task<IActionResult> GetMyMarks()
        {
            var userId = GetUserId();

            var profile = await _context.StudentProfiles
                .FirstOrDefaultAsync(sp => sp.UserId == userId);

            if (profile == null)
                return NotFound(new { message = "Profile maldo nahi." });

            var marks = await _context.InternalMarks
                .Include(im => im.Course)
                .Where(im => im.StudentProfileId == profile.StudentProfileId)
                .Select(im => new
                {
                    Subject = im.Course!.CourseName,
                    SubjectCode = im.Course.CourseCode,
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

            return Ok(marks);
        }
    }
}