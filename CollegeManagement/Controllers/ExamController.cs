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
    public class ExamController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ExamController(AppDbContext context)
        {
            _context = context;
        }

        private int GetUserId()
        {
            return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        }

        private static readonly HashSet<string> ValidExamTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            "Internal1", "Internal2", "External"
        };

        private async Task<StudentProfile?> GetStudentProfileAsync()
        {
            var userId = GetUserId();
            return await _context.StudentProfiles.FirstOrDefaultAsync(sp => sp.UserId == userId);
        }

        private async Task<bool> IsStudentEnrolledAsync(int studentProfileId, int courseId)
        {
            return await _context.Enrollments
                .AnyAsync(e => e.StudentProfileId == studentProfileId && e.CourseId == courseId);
        }

        private static object BuildExamSessionResponse(ExamAttempt attempt, Exam exam)
        {
            var questions = exam.Questions
                .Select(q => new
                {
                    q.QuestionId,
                    q.QuestionText,
                    q.OptionA,
                    q.OptionB,
                    q.OptionC,
                    q.OptionD,
                    q.Marks
                })
                .ToList();

            return new
            {
                attempt.AttemptId,
                exam.ExamId,
                exam.Title,
                exam.ExamType,
                exam.TotalMarks,
                exam.EndDateTime,
                Questions = questions
            };
        }

        // POST: api/Exam/create
        [Authorize(Roles = "Admin,Staff,Faculty")]
        [HttpPost("create")]
        public async Task<IActionResult> CreateExam(CreateExamDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Title))
                return BadRequest(new { message = "Exam title required che." });

            if (!ValidExamTypes.Contains(dto.ExamType))
                return BadRequest(new { message = "ExamType Internal1, Internal2, athva External hovu joiye." });

            // §3 — Enforce TotalMarks per ExamType
            if (dto.ExamType.Equals("Internal1", StringComparison.OrdinalIgnoreCase) ||
                dto.ExamType.Equals("Internal2", StringComparison.OrdinalIgnoreCase))
            {
                if (dto.TotalMarks != 20)
                    return BadRequest(new { message = "Internal exam na TotalMarks fixed 20 j hova joiye." });
            }
            else if (dto.ExamType.Equals("External", StringComparison.OrdinalIgnoreCase))
            {
                if (dto.TotalMarks != 60)
                    return BadRequest(new { message = "External exam na TotalMarks fixed 60 j hova joiye." });
            }

            if (string.IsNullOrWhiteSpace(dto.SecretCode))
                return BadRequest(new { message = "Secret code required che." });

            if (dto.StartDateTime >= dto.EndDateTime)
                return BadRequest(new { message = "End date/time start date/time pachhi hovu joiye." });

            var courseExists = await _context.Courses.AnyAsync(c => c.CourseId == dto.CourseId);
            if (!courseExists)
                return NotFound(new { message = "Subject maldo nahi." });

            var exam = new Exam
            {
                Title = dto.Title,
                CourseId = dto.CourseId,
                ExamType = dto.ExamType,
                TotalMarks = dto.TotalMarks,
                SecretCode = dto.SecretCode.Trim().ToUpperInvariant(),
                StartDateTime = dto.StartDateTime,
                EndDateTime = dto.EndDateTime,
                IsActive = true,
                CreatedByUserId = GetUserId()
            };

            _context.Exams.Add(exam);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Exam create thayo!",
                exam.ExamId,
                exam.Title,
                exam.SecretCode
            });
        }

        // POST: api/Exam/add-question
        [Authorize(Roles = "Admin,Staff,Faculty")]
        [HttpPost("add-question")]
        public async Task<IActionResult> AddQuestion(AddQuestionDTO dto)
        {
            var exam = await _context.Exams.FindAsync(dto.ExamId);
            if (exam == null)
                return NotFound(new { message = "Exam maldo nahi." });

            if (dto.CorrectOption != "A" && dto.CorrectOption != "B" &&
                dto.CorrectOption != "C" && dto.CorrectOption != "D")
                return BadRequest(new { message = "CorrectOption A, B, C, D mathi j hovu joiye." });

            var question = new Question
            {
                ExamId = dto.ExamId,
                QuestionText = dto.QuestionText,
                OptionA = dto.OptionA,
                OptionB = dto.OptionB,
                OptionC = dto.OptionC,
                OptionD = dto.OptionD,
                CorrectOption = dto.CorrectOption,
                Marks = dto.Marks
            };

            _context.Questions.Add(question);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Question add thayo!", question.QuestionId });
        }

        // GET: api/Exam/list/{courseId}
        [Authorize(Roles = "Admin,Staff,Faculty,Student")]
        [HttpGet("list/{courseId}")]
        public async Task<IActionResult> GetExamsByCourse(int courseId)
        {
            var isStaff = User.IsInRole("Admin") || User.IsInRole("Staff") || User.IsInRole("Faculty");

            if (User.IsInRole("Student"))
            {
                var profile = await GetStudentProfileAsync();
                if (profile == null)
                    return NotFound(new { message = "Profile maldo nahi." });

                if (!await IsStudentEnrolledAsync(profile.StudentProfileId, courseId))
                    return Forbid();
            }

            var exams = await _context.Exams
                .Where(e => e.CourseId == courseId)
                .OrderByDescending(e => e.StartDateTime)
                .Select(e => new
                {
                    e.ExamId,
                    e.Title,
                    e.ExamType,
                    e.TotalMarks,
                    e.StartDateTime,
                    e.EndDateTime,
                    e.IsActive,
                    QuestionCount = e.Questions.Count,
                    SecretCode = isStaff ? e.SecretCode : null
                })
                .ToListAsync();

            return Ok(exams);
        }

        // DELETE: api/Exam/{examId}
        [Authorize(Roles = "Admin,Staff,Faculty")]
        [HttpDelete("{examId}")]
        public async Task<IActionResult> DeleteExam(int examId)
        {
            var exam = await _context.Exams
                .Include(e => e.Questions)
                .Include(e => e.Attempts)
                    .ThenInclude(a => a.Answers)
                .FirstOrDefaultAsync(e => e.ExamId == examId);

            if (exam == null)
                return NotFound(new { message = "Exam maldo nahi." });

            foreach (var attempt in exam.Attempts)
                _context.ExamAnswers.RemoveRange(attempt.Answers);

            _context.ExamAttempts.RemoveRange(exam.Attempts);
            _context.Questions.RemoveRange(exam.Questions);
            _context.Exams.Remove(exam);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Exam delete thayo!" });
        }

        // POST: api/Exam/join
        // §4 — 5-minute join window + fee gating
        [Authorize(Roles = "Student")]
        [HttpPost("join")]
        public async Task<IActionResult> JoinExam(JoinExamDTO dto)
        {
            var userId = GetUserId();

            var profile = await _context.StudentProfiles
                .FirstOrDefaultAsync(sp => sp.UserId == userId && sp.EnrollmentNo == dto.EnrollmentNo);

            if (profile == null)
                return NotFound(new { message = "Student profile maldo nahi." });

            // §4 — Fee gating: any unpaid fee blocks exam access
            var hasUnpaidFees = await _context.FeeRecords
                .AnyAsync(f => f.StudentProfileId == profile.StudentProfileId && !f.IsPaid);

            if (hasUnpaidFees)
                return BadRequest(new { message = "Tamari fee complete nathi thai, exam ma join thai shakay nahi. Pehla fee bharo." });

            var exam = await _context.Exams
                .Include(e => e.Questions)
                .FirstOrDefaultAsync(e => e.ExamId == dto.ExamId);

            if (exam == null)
                return NotFound(new { message = "Exam maldo nahi." });

            if (!exam.IsActive)
                return BadRequest(new { message = "Aa exam active nathi." });

            if (!await IsStudentEnrolledAsync(profile.StudentProfileId, exam.CourseId))
                return Forbid();

            var secretCode = dto.SecretCode.Trim().ToUpperInvariant();
            if (!exam.SecretCode.Equals(secretCode, StringComparison.OrdinalIgnoreCase))
                return BadRequest(new { message = "Secret code khoto che." });

            if (exam.Questions.Count == 0)
                return BadRequest(new { message = "Aa exam ma koi question nathi. Faculty ne contact karo." });

            var now = DateTime.UtcNow;

            // §4 — 5-minute join window check
            var windowStart = exam.StartDateTime.AddMinutes(-5);
            var windowEnd = exam.StartDateTime.AddMinutes(5);

            if (now < windowStart)
                return BadRequest(new { message = "Exam haju shuru nathi thayo, 5 minute pehla thi join thai sakse." });

            if (now > windowEnd)
                return BadRequest(new { message = "Join karva no samay puro thai gayo che." });

            if (now > exam.EndDateTime)
                return BadRequest(new { message = "Exam no samay puro thai gayo che." });

            var existingAttempt = await _context.ExamAttempts
                .FirstOrDefaultAsync(a => a.ExamId == dto.ExamId && a.StudentProfileId == profile.StudentProfileId);

            if (existingAttempt != null)
            {
                if (existingAttempt.IsSubmitted)
                    return BadRequest(new { message = "Tame aa exam pehela thi j submit karyo che. Bije vaar attempt thai shake nahi." });

                return Ok(BuildExamSessionResponse(existingAttempt, exam));
            }

            var attempt = new ExamAttempt
            {
                ExamId = exam.ExamId,
                StudentProfileId = profile.StudentProfileId,
                StartedAt = now,
                IsSubmitted = false,
                TotalObtained = 0
            };

            _context.ExamAttempts.Add(attempt);
            await _context.SaveChangesAsync();

            return Ok(BuildExamSessionResponse(attempt, exam));
        }

        // POST: api/Exam/submit
        [Authorize(Roles = "Student")]
        [HttpPost("submit")]
        public async Task<IActionResult> SubmitExam(SubmitExamDTO dto)
        {
            var userId = GetUserId();

            var attempt = await _context.ExamAttempts
                .Include(a => a.Exam)
                .Include(a => a.StudentProfile)
                .FirstOrDefaultAsync(a => a.AttemptId == dto.AttemptId);

            if (attempt == null)
                return NotFound(new { message = "Attempt maldo nahi." });

            if (attempt.StudentProfile == null || attempt.StudentProfile.UserId != userId)
                return Forbid();

            if (attempt.IsSubmitted)
                return BadRequest(new { message = "Aa exam pehela thi j submit thai gayo che." });

            if (attempt.Exam == null)
                return NotFound(new { message = "Exam maldo nahi." });

            if (DateTime.UtcNow > attempt.Exam.EndDateTime)
                return BadRequest(new { message = "Exam no samay puro thai gayo che. Submit thai shake nahi." });

            var questionIds = dto.Answers.Select(a => a.QuestionId).ToList();
            var questions = await _context.Questions
                .Where(q => questionIds.Contains(q.QuestionId) && q.ExamId == attempt.ExamId)
                .ToListAsync();

            decimal totalObtained = 0;

            foreach (var ans in dto.Answers)
            {
                var question = questions.FirstOrDefault(q => q.QuestionId == ans.QuestionId);
                if (question == null) continue;

                bool isCorrect = question.CorrectOption.Equals(ans.SelectedOption, StringComparison.OrdinalIgnoreCase);

                if (isCorrect)
                    totalObtained += question.Marks;

                var examAnswer = new ExamAnswer
                {
                    AttemptId = attempt.AttemptId,
                    QuestionId = ans.QuestionId,
                    SelectedOption = ans.SelectedOption,
                    IsCorrect = isCorrect
                };

                _context.ExamAnswers.Add(examAnswer);
            }

            // §3 — Cap scores per ExamType
            if (attempt.Exam.ExamType.Equals("Internal1", StringComparison.OrdinalIgnoreCase) ||
                attempt.Exam.ExamType.Equals("Internal2", StringComparison.OrdinalIgnoreCase))
            {
                totalObtained = Math.Min(totalObtained, 20);
            }
            else if (attempt.Exam.ExamType.Equals("External", StringComparison.OrdinalIgnoreCase))
            {
                totalObtained = Math.Min(totalObtained, 60);
            }

            attempt.TotalObtained = totalObtained;
            attempt.IsSubmitted = true;
            attempt.SubmittedAt = DateTime.UtcNow;

            // §3 — Update Internal / External marks based on exam type
            if (attempt.Exam.ExamType.Equals("Internal1", StringComparison.OrdinalIgnoreCase) ||
                attempt.Exam.ExamType.Equals("Internal2", StringComparison.OrdinalIgnoreCase))
            {
                var internalMark = await _context.InternalMarks
                    .FirstOrDefaultAsync(im => im.StudentProfileId == attempt.StudentProfileId
                                             && im.CourseId == attempt.Exam.CourseId);

                if (internalMark == null)
                {
                    internalMark = new InternalMark
                    {
                        StudentProfileId = attempt.StudentProfileId,
                        CourseId = attempt.Exam.CourseId
                    };
                    _context.InternalMarks.Add(internalMark);
                }

                if (attempt.Exam.ExamType.Equals("Internal1", StringComparison.OrdinalIgnoreCase))
                    internalMark.Internal1Score = totalObtained;
                else
                    internalMark.Internal2Score = totalObtained;

                internalMark.BestInternalScore = Math.Max(internalMark.Internal1Score, internalMark.Internal2Score);

                // §3 — Updated formula with FacultyDiscretionMarks
                internalMark.TotalInternal =
                    internalMark.BestInternalScore +
                    internalMark.AttendanceMarks +
                    internalMark.QuizMarks +
                    internalMark.FacultyDiscretionMarks;

                if (internalMark.TotalInternal > 40)
                    internalMark.TotalInternal = 40;
            }
            else if (attempt.Exam.ExamType.Equals("External", StringComparison.OrdinalIgnoreCase))
            {
                var externalMark = await _context.ExternalMarks
                    .FirstOrDefaultAsync(em => em.StudentProfileId == attempt.StudentProfileId
                                             && em.CourseId == attempt.Exam.CourseId);

                if (externalMark == null)
                {
                    externalMark = new ExternalMark
                    {
                        StudentProfileId = attempt.StudentProfileId,
                        CourseId = attempt.Exam.CourseId
                    };
                    _context.ExternalMarks.Add(externalMark);
                }

                externalMark.ExternalScore = Math.Min(totalObtained, 60);
            }

            // Notification
            var notification = new Notification
            {
                UserId = userId,
                Title = "Exam Result",
                Message = $"Tamne '{attempt.Exam.Title}' exam ma {totalObtained}/{attempt.Exam.TotalMarks} marks malya che.",
                Type = "Exam",
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };
            _context.Notifications.Add(notification);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Exam submit thayo!",
                TotalObtained = totalObtained,
                TotalMarks = attempt.Exam.TotalMarks,
                Percentage = attempt.Exam.TotalMarks > 0
                    ? Math.Round((double)(totalObtained / attempt.Exam.TotalMarks) * 100, 2)
                    : 0
            });
        }

        // GET: api/Exam/my-attempts
        [Authorize(Roles = "Student")]
        [HttpGet("my-attempts")]
        public async Task<IActionResult> GetMyAttempts()
        {
            var userId = GetUserId();

            var profile = await _context.StudentProfiles
                .FirstOrDefaultAsync(sp => sp.UserId == userId);

            if (profile == null)
                return NotFound(new { message = "Profile maldo nahi." });

            var attempts = await _context.ExamAttempts
                .Include(a => a.Exam)
                    .ThenInclude(e => e!.Course)
                .Where(a => a.StudentProfileId == profile.StudentProfileId)
                .OrderByDescending(a => a.StartedAt)
                .Select(a => new
                {
                    a.AttemptId,
                    ExamTitle = a.Exam!.Title,
                    ExamType = a.Exam.ExamType,
                    Subject = a.Exam.Course!.CourseName,
                    Semester = a.Exam.Course.Semester,
                    a.TotalObtained,
                    TotalMarks = a.Exam.TotalMarks,
                    a.IsSubmitted,
                    a.StartedAt,
                    a.SubmittedAt,
                    Percentage = a.Exam.TotalMarks > 0
                        ? Math.Round((double)(a.TotalObtained / a.Exam.TotalMarks) * 100, 2)
                        : 0
                })
                .ToListAsync();

            return Ok(attempts);
        }

        // GET: api/Exam/{examId}/attempts
        [Authorize(Roles = "Admin,Staff,Faculty")]
        [HttpGet("{examId}/attempts")]
        public async Task<IActionResult> GetExamAttempts(int examId)
        {
            var exam = await _context.Exams.FindAsync(examId);
            if (exam == null)
                return NotFound(new { message = "Exam maldo nahi." });

            var attempts = await _context.ExamAttempts
                .Include(a => a.StudentProfile)
                    .ThenInclude(sp => sp!.User)
                .Where(a => a.ExamId == examId)
                .OrderByDescending(a => a.StartedAt)
                .Select(a => new
                {
                    a.AttemptId,
                    StudentName = a.StudentProfile!.User!.Name,
                    EnrollmentNo = a.StudentProfile.EnrollmentNo,
                    a.StartedAt,
                    a.SubmittedAt,
                    a.TotalObtained,
                    TotalMarks = exam.TotalMarks,
                    a.IsSubmitted,
                    Percentage = exam.TotalMarks > 0
                        ? Math.Round((double)(a.TotalObtained / exam.TotalMarks) * 100, 2)
                        : 0
                })
                .ToListAsync();

            return Ok(attempts);
        }
    }
}