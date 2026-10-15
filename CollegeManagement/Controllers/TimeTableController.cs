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
    public class TimeTableController : ControllerBase
    {
        private readonly AppDbContext _context;
        public TimeTableController(AppDbContext context) => _context = context;

        private int GetUserId() =>
            int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        private static readonly Dictionary<int, string> DayNames = new()
        {
            { 1, "Monday" }, { 2, "Tuesday" }, { 3, "Wednesday" },
            { 4, "Thursday" }, { 5, "Friday" }, { 6, "Saturday" }
        };

        // ── POST: api/TimeTable ── Admin creates an entry
        [Authorize(Roles = "Admin,Staff")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] TimeTableEntryDTO dto)
        {
            if (dto.DayOfWeek < 1 || dto.DayOfWeek > 6)
                return BadRequest(new { message = "DayOfWeek 1 (Monday) thi 6 (Saturday) ni vachhe hovu joiye." });

            if (dto.StartTime >= dto.EndTime)
                return BadRequest(new { message = "EndTime StartTime pachhi hovu joiye." });

            // Validate dept + semester + day: no slot overlap for same dept/sem
            var deptConflict = await _context.TimeTableEntries
                .AnyAsync(t => t.DepartmentId == dto.DepartmentId
                            && t.Semester == dto.Semester
                            && t.DayOfWeek == dto.DayOfWeek
                            && t.StartTime < dto.EndTime
                            && dto.StartTime < t.EndTime);

            if (deptConflict)
                return Conflict(new { message = "Aa department/semester/din mate time slot already booked che." });

            // Validate faculty not double-booked at same time
            var facultyConflict = await _context.TimeTableEntries
                .AnyAsync(t => t.FacultyUserId == dto.FacultyUserId
                            && t.DayOfWeek == dto.DayOfWeek
                            && t.StartTime < dto.EndTime
                            && dto.StartTime < t.EndTime);

            if (facultyConflict)
                return Conflict(new { message = "Faculty aa same time slot ma biji jagya busy che." });

            var entry = new TimeTableEntry
            {
                DepartmentId = dto.DepartmentId,
                Semester = dto.Semester,
                CourseId = dto.CourseId,
                DayOfWeek = dto.DayOfWeek,
                StartTime = dto.StartTime,
                EndTime = dto.EndTime,
                RoomNumber = dto.RoomNumber,
                FacultyUserId = dto.FacultyUserId
            };

            _context.TimeTableEntries.Add(entry);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Time table entry add thayo!", entry.EntryId });
        }

        // ── DELETE: api/TimeTable/{id} ── Admin deletes an entry
        [Authorize(Roles = "Admin,Staff")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var entry = await _context.TimeTableEntries.FindAsync(id);
            if (entry == null)
                return NotFound(new { message = "Entry maldi nahi." });

            _context.TimeTableEntries.Remove(entry);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Entry delete thayo." });
        }

        // ── GET: api/TimeTable/{departmentId}/{semester} ── Full timetable for dept/sem
        [Authorize]
        [HttpGet("{departmentId}/{semester}")]
        public async Task<IActionResult> GetByDeptSemester(int departmentId, int semester)
        {
            var entries = await _context.TimeTableEntries
                .Include(t => t.Course)
                .Where(t => t.DepartmentId == departmentId && t.Semester == semester)
                .OrderBy(t => t.DayOfWeek).ThenBy(t => t.StartTime)
                .Select(t => new
                {
                    t.EntryId,
                    Day = DayNames.ContainsKey(t.DayOfWeek) ? DayNames[t.DayOfWeek] : t.DayOfWeek.ToString(),
                    t.DayOfWeek,
                    StartTime = t.StartTime.ToString(@"hh\:mm"),
                    EndTime = t.EndTime.ToString(@"hh\:mm"),
                    Subject = t.Course!.CourseName,
                    SubjectCode = t.Course.CourseCode,
                    t.RoomNumber,
                    t.FacultyUserId
                })
                .ToListAsync();

            return Ok(entries);
        }

        // ── GET: api/TimeTable/my ── Student's own timetable (via their dept + semester)
        [Authorize(Roles = "Student")]
        [HttpGet("my")]
        public async Task<IActionResult> GetMyTimeTable()
        {
            var userId = GetUserId();
            var profile = await _context.StudentProfiles
                .FirstOrDefaultAsync(sp => sp.UserId == userId);

            if (profile == null)
                return NotFound(new { message = "Profile maldo nahi." });

            return await GetByDeptSemester(profile.DepartmentId, profile.Semester);
        }

        // ── GET: api/TimeTable/my-schedule ── Faculty's own teaching schedule
        [Authorize(Roles = "Staff,Faculty")]
        [HttpGet("my-schedule")]
        public async Task<IActionResult> GetMySchedule()
        {
            var userId = GetUserId();

            var entries = await _context.TimeTableEntries
                .Include(t => t.Course)
                .Include(t => t.Department)
                .Where(t => t.FacultyUserId == userId)
                .OrderBy(t => t.DayOfWeek).ThenBy(t => t.StartTime)
                .Select(t => new
                {
                    t.EntryId,
                    Day = DayNames.ContainsKey(t.DayOfWeek) ? DayNames[t.DayOfWeek] : t.DayOfWeek.ToString(),
                    t.DayOfWeek,
                    StartTime = t.StartTime.ToString(@"hh\:mm"),
                    EndTime = t.EndTime.ToString(@"hh\:mm"),
                    Subject = t.Course!.CourseName,
                    DepartmentName = t.Department!.DepartmentName,
                    t.Semester,
                    t.RoomNumber
                })
                .ToListAsync();

            return Ok(entries);
        }
    }
}
