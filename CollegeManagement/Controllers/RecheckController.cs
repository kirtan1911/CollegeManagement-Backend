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
    public class RecheckController : ControllerBase
    {
        private readonly AppDbContext _context;
        public RecheckController(AppDbContext context) => _context = context;

        private int GetUserId() =>
            int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        // ── POST: api/Recheck/request ── Student raises a re-check request
        [Authorize(Roles = "Student")]
        [HttpPost("request")]
        public async Task<IActionResult> RaiseRequest([FromBody] RecheckRequestDTO dto)
        {
            var userId = GetUserId();
            var profile = await _context.StudentProfiles
                .FirstOrDefaultAsync(sp => sp.UserId == userId);

            if (profile == null)
                return NotFound(new { message = "Profile maldo nahi." });

            // Find the exam attempt to get current marks
            var attempt = await _context.ExamAttempts
                .Include(a => a.Exam)
                .FirstOrDefaultAsync(a => a.ExamId == dto.ExamId
                                       && a.StudentProfileId == profile.StudentProfileId
                                       && a.IsSubmitted);

            if (attempt == null)
                return NotFound(new { message = "Submitted exam attempt maldo nahi." });

            // 7-day window default
            var cutoff = attempt.SubmittedAt?.AddDays(7) ?? DateTime.MinValue;
            if (DateTime.UtcNow > cutoff)
                return BadRequest(new { message = "Re-check request maat no samay (7 din) puro thai gayo che." });

            // No duplicate pending request for same exam
            var dup = await _context.RecheckRequests
                .AnyAsync(r => r.StudentProfileId == profile.StudentProfileId
                              && r.ExamId == dto.ExamId
                              && r.Status == "Pending");
            if (dup)
                return Conflict(new { message = "Aa exam mate already pending re-check request che." });

            var request = new RecheckRequest
            {
                StudentProfileId = profile.StudentProfileId,
                ExamId = dto.ExamId,
                RequestDate = DateTime.UtcNow,
                Reason = dto.Reason,
                Status = "Pending",
                OldMarks = attempt.TotalObtained
            };

            _context.RecheckRequests.Add(request);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Re-check request submit thayo!", request.RequestId });
        }

        // ── GET: api/Recheck/pending ── Admin/Staff see all pending requests
        [Authorize(Roles = "Admin,Staff,Faculty")]
        [HttpGet("pending")]
        public async Task<IActionResult> GetPending()
        {
            var requests = await _context.RecheckRequests
                .Include(r => r.StudentProfile).ThenInclude(sp => sp!.User)
                .Include(r => r.Exam).ThenInclude(e => e!.Course)
                .Where(r => r.Status == "Pending")
                .OrderBy(r => r.RequestDate)
                .Select(r => new
                {
                    r.RequestId,
                    StudentName = r.StudentProfile!.User!.Name,
                    EnrollmentNo = r.StudentProfile.EnrollmentNo,
                    ExamTitle = r.Exam!.Title,
                    ExamType = r.Exam.ExamType,
                    Subject = r.Exam.Course!.CourseName,
                    r.OldMarks,
                    r.Reason,
                    r.RequestDate
                })
                .ToListAsync();

            return Ok(requests);
        }

        // ── PUT: api/Recheck/process/{requestId} ── Admin/Staff approves or rejects
        [Authorize(Roles = "Admin,Staff,Faculty")]
        [HttpPut("process/{requestId}")]
        public async Task<IActionResult> ProcessRequest(int requestId, [FromBody] ProcessRecheckDTO dto)
        {
            if (dto.Status != "Approved" && dto.Status != "Rejected")
                return BadRequest(new { message = "Status Approved athva Rejected j hovu joiye." });

            var request = await _context.RecheckRequests
                .Include(r => r.Exam)
                .Include(r => r.StudentProfile).ThenInclude(sp => sp!.User)
                .FirstOrDefaultAsync(r => r.RequestId == requestId);

            if (request == null)
                return NotFound(new { message = "Re-check request maldo nahi." });

            if (request.Status != "Pending")
                return BadRequest(new { message = "Aa request pahela thi j process thai gayo che." });

            request.Status = dto.Status;
            request.ProcessedByUserId = GetUserId();
            request.ProcessedDate = DateTime.UtcNow;

            if (dto.Status == "Approved" && dto.NewMarks.HasValue)
            {
                request.NewMarks = dto.NewMarks.Value;

                // Update the underlying marks based on ExamType
                var exam = request.Exam!;
                if (exam.ExamType.Equals("Internal1", StringComparison.OrdinalIgnoreCase) ||
                    exam.ExamType.Equals("Internal2", StringComparison.OrdinalIgnoreCase))
                {
                    var im = await _context.InternalMarks
                        .FirstOrDefaultAsync(i => i.StudentProfileId == request.StudentProfileId
                                               && i.CourseId == exam.CourseId);
                    if (im != null)
                    {
                        if (exam.ExamType.Equals("Internal1", StringComparison.OrdinalIgnoreCase))
                            im.Internal1Score = dto.NewMarks.Value;
                        else
                            im.Internal2Score = dto.NewMarks.Value;

                        im.BestInternalScore = Math.Max(im.Internal1Score, im.Internal2Score);
                        im.TotalInternal = Math.Min(
                            im.BestInternalScore + im.AttendanceMarks + im.QuizMarks + im.FacultyDiscretionMarks,
                            40);
                    }
                }
                else if (exam.ExamType.Equals("External", StringComparison.OrdinalIgnoreCase))
                {
                    var em = await _context.ExternalMarks
                        .FirstOrDefaultAsync(e => e.StudentProfileId == request.StudentProfileId
                                               && e.CourseId == exam.CourseId);
                    if (em != null)
                        em.ExternalScore = Math.Min(dto.NewMarks.Value, 60);
                }

                // ── Fix M4: Also update ExamAttempt.TotalObtained to reflect new marks ──
                var attempt = await _context.ExamAttempts
                    .FirstOrDefaultAsync(a => a.ExamId == request.ExamId
                                           && a.StudentProfileId == request.StudentProfileId
                                           && a.IsSubmitted);
                if (attempt != null)
                    attempt.TotalObtained = dto.NewMarks.Value;

                // Notify student
                var studentUserId = request.StudentProfile?.UserId ?? 0;
                if (studentUserId > 0)
                {
                    _context.Notifications.Add(new Notification
                    {
                        UserId = studentUserId,
                        Title = "Re-check Result",
                        Message = $"Your re-check request for '{exam.Title}' has been approved. New marks: {dto.NewMarks.Value}.",
                        Type = "Recheck",
                        IsRead = false,
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }
            else if (dto.Status == "Rejected")
            {
                var studentUserId = request.StudentProfile?.UserId ?? 0;
                if (studentUserId > 0)
                {
                    _context.Notifications.Add(new Notification
                    {
                        UserId = studentUserId,
                        Title = "Re-check Result",
                        Message = $"Tamaro '{request.Exam?.Title}' exam no re-check request reject thayo.",
                        Type = "Recheck",
                        IsRead = false,
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = $"Re-check request {dto.Status} thayo.", request.RequestId });
        }

        // ── GET: api/Recheck/my-requests ── Student's own requests
        [Authorize(Roles = "Student")]
        [HttpGet("my-requests")]
        public async Task<IActionResult> GetMyRequests()
        {
            var userId = GetUserId();
            var profile = await _context.StudentProfiles
                .FirstOrDefaultAsync(sp => sp.UserId == userId);

            if (profile == null)
                return NotFound(new { message = "Profile maldo nahi." });

            var requests = await _context.RecheckRequests
                .Include(r => r.Exam).ThenInclude(e => e!.Course)
                .Where(r => r.StudentProfileId == profile.StudentProfileId)
                .OrderByDescending(r => r.RequestDate)
                .Select(r => new
                {
                    r.RequestId,
                    ExamTitle = r.Exam!.Title,
                    Subject = r.Exam.Course!.CourseName,
                    r.OldMarks,
                    r.NewMarks,
                    r.Status,
                    r.RequestDate,
                    r.ProcessedDate,
                    r.Reason
                })
                .ToListAsync();

            return Ok(requests);
        }
    }
}
