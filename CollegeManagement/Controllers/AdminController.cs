using CollegeManagementSystem.Data;
using CollegeManagementSystem.DTOs;
using CollegeManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CollegeManagementSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AdminController(AppDbContext context)
        {
            _context = context;
        }

        // =====================================================
        // 📊 DASHBOARD
        // =====================================================

        // GET: api/Admin/dashboard
        [HttpGet("dashboard")]
        public async Task<IActionResult> Dashboard()
        {
            var totalUsers = await _context.Users.CountAsync();
            var totalStudents = await _context.Users.CountAsync(x => x.RoleId == 3);
            var totalStaff = await _context.Users.CountAsync(x => x.RoleId == 2);
            var totalAdmins = await _context.Users.CountAsync(x => x.RoleId == 1);

            var totalDepartments = await _context.Departments.CountAsync();
            var totalCourses = await _context.Courses.CountAsync();
            var totalEnrollments = await _context.Enrollments.CountAsync();

            return Ok(new
            {
                TotalUsers = totalUsers,
                TotalStudents = totalStudents,
                TotalStaff = totalStaff,
                TotalAdmins = totalAdmins,
                TotalDepartments = totalDepartments,
                TotalCourses = totalCourses,
                TotalEnrollments = totalEnrollments
            });
        }

        // GET: api/Admin/total-users
        [HttpGet("total-users")]
        public async Task<IActionResult> TotalUsers()
        {
            var total = await _context.Users.CountAsync();
            return Ok(total);
        }

        // GET: api/Admin/total-students
        [HttpGet("total-students")]
        public async Task<IActionResult> TotalStudents()
        {
            var total = await _context.Users.CountAsync(x => x.RoleId == 3);
            return Ok(total);
        }

        // GET: api/Admin/total-staff
        [HttpGet("total-staff")]
        public async Task<IActionResult> TotalStaff()
        {
            var total = await _context.Users.CountAsync(x => x.RoleId == 2);
            return Ok(total);
        }

        // =====================================================
        // 👥 USER MANAGEMENT
        // =====================================================

        // GET: api/Admin/users
        [HttpGet("users")]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _context.Users
                .Include(x => x.Role)
                .Select(x => new
                {
                    x.UserId,
                    x.Name,
                    x.Email,
                    x.Status,
                    RoleName = x.Role != null ? x.Role.RoleName : "Unknown"
                })
                .ToListAsync();
            return Ok(users);
        }

        // PUT: api/Admin/change-role/5?roleId=2
        [HttpPut("change-role/{id}")]
        public async Task<IActionResult> ChangeRole(int id, int roleId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.UserId == id);
            if (user == null) return NotFound("User Not Found");

            // Validate the roleId exists in the Roles table
            var roleExists = await _context.Roles.AnyAsync(r => r.RoleId == roleId);
            if (!roleExists)
                return BadRequest(new { message = $"Role with ID {roleId} does not exist." });

            user.RoleId = roleId;
            await _context.SaveChangesAsync();
            return Ok("Role Updated Successfully");
        }

        // PUT: api/Admin/change-status/5
        [HttpPut("change-status/{id}")]
        public async Task<IActionResult> ChangeStatus(int id)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.UserId == id);
            if (user == null) return NotFound("User Not Found");

            user.Status = !user.Status;
            await _context.SaveChangesAsync();

            return Ok(new { user.UserId, user.Name, user.Status });
        }

        // PUT: api/Admin/update-user/5
        [HttpPut("update-user/{id}")]
        public async Task<IActionResult> UpdateUser(int id, UpdateUserDTO dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.UserId == id);
            if (user == null) return NotFound("User Not Found");

            user.Name = dto.Name;
            user.Email = dto.Email;
            user.Status = dto.Status;

            // "Staff" આવે તો "Faculty" સમજો
            var roleNameToSearch = dto.RoleName == "Staff" ? "Faculty" : dto.RoleName;

            var role = await _context.Roles.FirstOrDefaultAsync(x => x.RoleName.ToLower() == roleNameToSearch.ToLower());
            if (role != null) user.RoleId = role.RoleId;

            await _context.SaveChangesAsync();
            return Ok("User Updated Successfully");
        }

        // DELETE: api/Admin/delete-user/5
        [HttpDelete("delete-user/{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.UserId == id);
            if (user == null) return NotFound("User Not Found");

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return Ok("User Deleted Successfully");
        }

        // =====================================================
        // 🏛️ DEPARTMENT MANAGEMENT
        // =====================================================

        // GET: api/Admin/departments
        [HttpGet("departments")]
        public async Task<IActionResult> GetDepartments()
        {
            var departments = await _context.Departments
                .Select(d => new
                {
                    d.DepartmentId,
                    d.DepartmentName,
                    CourseCount = d.Courses.Count()
                })
                .ToListAsync();

            return Ok(departments);
        }

        // POST: api/Admin/departments
        [HttpPost("departments")]
        public async Task<IActionResult> AddDepartment([FromBody] DepartmentDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.DepartmentName))
                return BadRequest(new { message = "Department name is required." });

            var normalised = dto.DepartmentName.Trim();

            // Case-insensitive duplicate check
            var exists = await _context.Departments
                .AnyAsync(d => d.DepartmentName.ToLower() == normalised.ToLower());
            if (exists)
                return Conflict(new { message = "A department with this name already exists." });

            var dept = new Department
            {
                DepartmentName = normalised
            };

            _context.Departments.Add(dept);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Department added successfully!", dept.DepartmentId, dept.DepartmentName });
        }

        // PUT: api/Admin/departments/5
        [HttpPut("departments/{id}")]
        public async Task<IActionResult> UpdateDepartment(int id, [FromBody] DepartmentDTO dto)
        {
            var dept = await _context.Departments.FindAsync(id);
            if (dept == null) return NotFound(new { message = "Department maldo nahi." });

            dept.DepartmentName = dto.DepartmentName ?? dept.DepartmentName;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Department update thayun!", dept.DepartmentId, dept.DepartmentName });
        }

        // DELETE: api/Admin/departments/5
        [HttpDelete("departments/{id}")]
        public async Task<IActionResult> DeleteDepartment(int id)
        {
            var dept = await _context.Departments.FindAsync(id);
            if (dept == null) return NotFound(new { message = "Department maldo nahi." });

            _context.Departments.Remove(dept);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Department delete thayun!" });
        }

        // =====================================================
        // 📚 COURSE MANAGEMENT
        // =====================================================

        // GET: api/Admin/courses?departmentId=1&semester=5
        [HttpGet("courses")]
        public async Task<IActionResult> GetCourses([FromQuery] int? departmentId, [FromQuery] int? semester)
        {
            var query = _context.Courses
                .Include(c => c.Department)
                .AsQueryable();

            if (departmentId.HasValue)
                query = query.Where(c => c.DepartmentId == departmentId.Value);

            if (semester.HasValue)
                query = query.Where(c => c.Semester == semester.Value);

            var courses = await query
                .Select(c => new
                {
                    c.CourseId,
                    c.CourseName,
                    c.CourseCode,
                    c.Credits,
                    c.Semester,
                    Department = c.Department != null ? c.Department.DepartmentName : "N/A",
                    c.DepartmentId,
                    EnrolledStudents = c.Enrollments != null ? c.Enrollments.Count() : 0
                })
                .OrderBy(c => c.Semester)
                .ThenBy(c => c.CourseName)
                .ToListAsync();

            return Ok(courses);
        }

        // POST: api/Admin/courses
        [HttpPost("courses")]
        public async Task<IActionResult> AddCourse([FromBody] CourseDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.CourseName) || string.IsNullOrWhiteSpace(dto.CourseCode))
                return BadRequest(new { message = "Subject name ane code required che." });

            if (dto.Semester < 1 || dto.Semester > 8)
                return BadRequest(new { message = "Semester 1 thi 8 ni vachhe hovo joiye." });

            var deptExists = await _context.Departments.AnyAsync(d => d.DepartmentId == dto.DepartmentId);
            if (!deptExists)
                return NotFound(new { message = "Department maldo nahi." });

            // Unique check: same dept + semester + code
            var codeExists = await _context.Courses.AnyAsync(c =>
                c.DepartmentId == dto.DepartmentId &&
                c.Semester == dto.Semester &&
                c.CourseCode == dto.CourseCode);
            if (codeExists)
                return Conflict(new { message = "Aa department + semester ma same code no subject pahela thi che." });

            var course = new Course
            {
                CourseName = dto.CourseName,
                CourseCode = dto.CourseCode,
                Credits = dto.Credits,
                Semester = dto.Semester,
                DepartmentId = dto.DepartmentId
            };

            _context.Courses.Add(course);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Subject successfully add thayun!", course.CourseId, course.CourseName, course.CourseCode, course.Semester });
        }

        // PUT: api/Admin/courses/5
        [HttpPut("courses/{id}")]
        public async Task<IActionResult> UpdateCourse(int id, [FromBody] CourseDTO dto)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course == null) return NotFound(new { message = "Subject maldo nahi." });

            // Check new unique constraint if code/dept/semester changes
            var newDeptId = dto.DepartmentId > 0 ? dto.DepartmentId : course.DepartmentId;
            var newSemester = dto.Semester > 0 ? dto.Semester : course.Semester;
            var newCode = !string.IsNullOrWhiteSpace(dto.CourseCode) ? dto.CourseCode : course.CourseCode;

            var conflict = await _context.Courses.AnyAsync(c =>
                c.CourseId != id &&
                c.DepartmentId == newDeptId &&
                c.Semester == newSemester &&
                c.CourseCode == newCode);
            if (conflict)
                return Conflict(new { message = "Aa department + semester ma same code no subject pahela thi che." });

            course.CourseName = !string.IsNullOrWhiteSpace(dto.CourseName) ? dto.CourseName : course.CourseName;
            course.CourseCode = newCode;
            course.Credits = dto.Credits > 0 ? dto.Credits : course.Credits;
            course.Semester = dto.Semester > 0 ? dto.Semester : course.Semester;
            if (dto.DepartmentId > 0) course.DepartmentId = dto.DepartmentId;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Subject update thayun!", course.CourseId, course.CourseName, course.Semester });
        }

        // DELETE: api/Admin/courses/5
        [HttpDelete("courses/{id}")]
        public async Task<IActionResult> DeleteCourse(int id)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course == null) return NotFound(new { message = "Course maldo nahi." });

            _context.Courses.Remove(course);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Course delete thayun!" });
        }

        // =====================================================
        // 🎓 STUDENT PROFILE MANAGEMENT
        // =====================================================

        // GET: api/Admin/students
        // GET: api/Admin/student-profiles  ✅ fixed route
[HttpGet("student-profiles")]
public async Task<IActionResult> GetStudentProfiles()
{
    var students = await _context.StudentProfiles
        .Include(s => s.User)
        .Include(s => s.Department)
        .Select(s => new
        {
            s.StudentProfileId,
            s.EnrollmentNo,
            s.Semester,
            StudentName = s.User != null ? s.User.Name : "Unknown",
            Email = s.User != null ? s.User.Email : "",
            Department = s.Department != null ? s.Department.DepartmentName : "Unknown",
            s.UserId,
            s.DepartmentId,
            // Phase 7 — Commute Details
            s.ResidenceType,
            s.HomeTown,
            s.TransportOpted,
            s.TransportRoute
        })
        .ToListAsync();

    return Ok(students);
}

// POST: api/Admin/student-profiles
[HttpPost("student-profiles")]
public async Task<IActionResult> CreateStudentProfile([FromBody] StudentProfileDTO dto)
{
    var user = await _context.Users.FindAsync(dto.UserId);
    if (user == null) return NotFound(new { message = "User not found." });

    // Ensure only users with the Student role (RoleId = 3) get a student profile
    if (user.RoleId != 3)
        return BadRequest(new { message = "Cannot create a student profile for a non-Student user. Change the user's role to Student first." });

    var profileExists = await _context.StudentProfiles
        .AnyAsync(s => s.UserId == dto.UserId);
    if (profileExists)
        return Conflict(new { message = "A student profile already exists for this user." });

    var rollExists = await _context.StudentProfiles
        .AnyAsync(s => s.EnrollmentNo == dto.EnrollmentNo);
    if (rollExists)
        return Conflict(new { message = "This enrollment number is already in use." });

    var profile = new StudentProfile
    {
        UserId = dto.UserId,
        EnrollmentNo = dto.EnrollmentNo,
        DepartmentId = dto.DepartmentId,
        Semester = dto.Semester,
        ResidenceType = dto.ResidenceType,
        HomeTown = dto.HomeTown,
        TransportOpted = dto.TransportOpted,
        TransportRoute = dto.TransportRoute
    };

    _context.StudentProfiles.Add(profile);
    await _context.SaveChangesAsync();

    return Ok(new { message = "Student profile created!", profile.StudentProfileId, profile.EnrollmentNo });
}

// PUT: api/Admin/student-profiles/{id} — update commute and academic details
[HttpPut("student-profiles/{id}")]
public async Task<IActionResult> UpdateStudentProfile(int id, [FromBody] StudentProfileDTO dto)
{
    var profile = await _context.StudentProfiles.FindAsync(id);
    if (profile == null) return NotFound(new { message = "Student profile not found." });

    // Check enrollment number uniqueness if it's being changed
    if (!string.IsNullOrWhiteSpace(dto.EnrollmentNo) &&
        dto.EnrollmentNo != profile.EnrollmentNo)
    {
        var enrollmentExists = await _context.StudentProfiles
            .AnyAsync(sp => sp.EnrollmentNo == dto.EnrollmentNo
                         && sp.StudentProfileId != id);
        if (enrollmentExists)
            return Conflict(new { message = "Enrollment number already exists for another student." });
    }

    // Validate department exists
    var deptExists = await _context.Departments.AnyAsync(d => d.DepartmentId == dto.DepartmentId);
    if (!deptExists)
        return NotFound(new { message = "Department not found." });

    profile.EnrollmentNo = dto.EnrollmentNo;
    profile.Semester = dto.Semester;
    profile.DepartmentId = dto.DepartmentId;
    // Phase 7 — Commute Details
    profile.ResidenceType  = dto.ResidenceType;
    profile.HomeTown       = dto.HomeTown?.Trim() ?? string.Empty;
    profile.TransportOpted = dto.TransportOpted;
    profile.TransportRoute = dto.TransportOpted ? dto.TransportRoute?.Trim() : null;

    await _context.SaveChangesAsync();
    return Ok(new { message = "Student profile updated successfully!" });
}

        // =====================================================
        // 🔗 ENROLLMENT MANAGEMENT
        // =====================================================

        // GET: api/Admin/enrollments
        [HttpGet("enrollments")]
        public async Task<IActionResult> GetEnrollments([FromQuery] int? studentProfileId)
        {
            var query = _context.Enrollments
                .Include(e => e.StudentProfile!).ThenInclude(s => s.User)
                .Include(e => e.StudentProfile!).ThenInclude(s => s.Department)
                .Include(e => e.Course)
                .AsQueryable();

            if (studentProfileId.HasValue)
                query = query.Where(e => e.StudentProfileId == studentProfileId.Value);

            var enrollments = await query
                .Select(e => new
                {
                    e.EnrollmentId,
                    e.EnrolledDate,
                    StudentName = e.StudentProfile != null && e.StudentProfile.User != null ? e.StudentProfile.User.Name : "Unknown",
                    EnrollmentNo = e.StudentProfile != null ? e.StudentProfile.EnrollmentNo : "",
                    CourseName = e.Course != null ? e.Course.CourseName : "Unknown",
                    CourseCode = e.Course != null ? e.Course.CourseCode : "",
                    Department = e.StudentProfile != null && e.StudentProfile.Department != null ? e.StudentProfile.Department.DepartmentName : "Unknown"
                })
                .ToListAsync();

            return Ok(enrollments);
        }

        // POST: api/Admin/enroll
        [HttpPost("enroll")]
        public async Task<IActionResult> EnrollStudent([FromBody] EnrollDTO dto)
        {
            var student = await _context.StudentProfiles.FindAsync(dto.StudentProfileId);
            if (student == null) return NotFound(new { message = "Student profile maldo nahi." });

            var course = await _context.Courses.FindAsync(dto.CourseId);
            if (course == null) return NotFound(new { message = "Course maldo nahi." });

            var alreadyEnrolled = await _context.Enrollments
                .AnyAsync(e => e.StudentProfileId == dto.StudentProfileId && e.CourseId == dto.CourseId);
            if (alreadyEnrolled)
                return Conflict(new { message = "Student already enroll che aa course ma." });

            var enrollment = new Enrollment
            {
                StudentProfileId = dto.StudentProfileId,
                CourseId = dto.CourseId,
                EnrolledDate = DateTime.UtcNow
            };

            _context.Enrollments.Add(enrollment);
            await _context.SaveChangesAsync();

            return Ok(new { message = $"Student successfully enrolled in '{course.CourseName}'!", enrollment.EnrollmentId });
        }

        // DELETE: api/Admin/enrollments/5
        [HttpDelete("enrollments/{id}")]
        public async Task<IActionResult> RemoveEnrollment(int id)
        {
            var enrollment = await _context.Enrollments.FindAsync(id);
            if (enrollment == null) return NotFound(new { message = "Enrollment maldi nahi." });

            _context.Enrollments.Remove(enrollment);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Enrollment remove thayun!" });
        }
    }
}