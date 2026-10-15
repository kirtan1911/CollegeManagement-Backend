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
    public class PlacementController : ControllerBase
    {
        private readonly AppDbContext _context;
        public PlacementController(AppDbContext context) => _context = context;

        private int GetUserId() =>
            int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        // ── GET: api/Placement/companies ── All active companies
        [Authorize(Roles = "Admin,Staff,Faculty,Student")]
        [HttpGet("companies")]
        public async Task<IActionResult> GetCompanies([FromQuery] bool? activeOnly)
        {
            var query = _context.Companies.AsQueryable();
            if (activeOnly == true)
                query = query.Where(c => c.IsActive);

            var companies = await query
                .Select(c => new
                {
                    c.CompanyId,
                    c.Name,
                    c.Industry,
                    c.Location,
                    c.EligibilityCriteria,
                    c.PackageLPA,
                    c.DriveDate,
                    c.IsActive,
                    c.Description,
                    ApplicantCount = c.Applications.Count
                })
                .OrderByDescending(c => c.DriveDate)
                .ToListAsync();

            return Ok(companies);
        }

        // ── POST: api/Placement/companies ── Admin creates company
        [Authorize(Roles = "Admin,Staff")]
        [HttpPost("companies")]
        public async Task<IActionResult> CreateCompany([FromBody] CompanyDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                return BadRequest(new { message = "Company name required che." });

            var company = new Company
            {
                Name = dto.Name,
                Industry = dto.Industry,
                Location = dto.Location,
                EligibilityCriteria = dto.EligibilityCriteria,
                PackageLPA = dto.PackageLPA,
                DriveDate = dto.DriveDate,
                Description = dto.Description,
                IsActive = dto.IsActive
            };
            _context.Companies.Add(company);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Company add thayo!", company.CompanyId });
        }

        // ── PUT: api/Placement/companies/{id} ── Admin updates company
        [Authorize(Roles = "Admin,Staff")]
        [HttpPut("companies/{id}")]
        public async Task<IActionResult> UpdateCompany(int id, [FromBody] CompanyDTO dto)
        {
            var company = await _context.Companies.FindAsync(id);
            if (company == null)
                return NotFound(new { message = "Company maldi nahi." });

            company.Name = dto.Name;
            company.Industry = dto.Industry;
            company.Location = dto.Location;
            company.EligibilityCriteria = dto.EligibilityCriteria;
            company.PackageLPA = dto.PackageLPA;
            company.DriveDate = dto.DriveDate;
            company.Description = dto.Description;
            company.IsActive = dto.IsActive;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Company update thayo!" });
        }

        // ── DELETE: api/Placement/companies/{id} ── Admin deletes company
        [Authorize(Roles = "Admin")]
        [HttpDelete("companies/{id}")]
        public async Task<IActionResult> DeleteCompany(int id)
        {
            var company = await _context.Companies.FindAsync(id);
            if (company == null)
                return NotFound(new { message = "Company maldi nahi." });

            _context.Companies.Remove(company);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Company delete thayo." });
        }

        // ── POST: api/Placement/apply/{companyId} ── Student applies
        [Authorize(Roles = "Student")]
        [HttpPost("apply/{companyId}")]
        public async Task<IActionResult> Apply(int companyId)
        {
            var userId = GetUserId();
            var profile = await _context.StudentProfiles
                .FirstOrDefaultAsync(sp => sp.UserId == userId);

            if (profile == null)
                return NotFound(new { message = "Profile maldo nahi." });

            var company = await _context.Companies.FindAsync(companyId);
            if (company == null || !company.IsActive)
                return NotFound(new { message = "Company maldi nahi athva active nathi." });

            var alreadyApplied = await _context.PlacementApplications
                .AnyAsync(a => a.StudentProfileId == profile.StudentProfileId && a.CompanyId == companyId);
            if (alreadyApplied)
                return Conflict(new { message = "Tame aa company mate pahela thi j apply karyu che." });

            var application = new PlacementApplication
            {
                StudentProfileId = profile.StudentProfileId,
                CompanyId = companyId,
                AppliedDate = DateTime.UtcNow,
                Status = "Applied"
            };
            _context.PlacementApplications.Add(application);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Application submit thayo!", application.ApplicationId });
        }

        // ── GET: api/Placement/my-applications ── Student's own applications
        [Authorize(Roles = "Student")]
        [HttpGet("my-applications")]
        public async Task<IActionResult> GetMyApplications()
        {
            var userId = GetUserId();
            var profile = await _context.StudentProfiles
                .FirstOrDefaultAsync(sp => sp.UserId == userId);

            if (profile == null)
                return NotFound(new { message = "Profile maldo nahi." });

            var apps = await _context.PlacementApplications
                .Include(a => a.Company)
                .Where(a => a.StudentProfileId == profile.StudentProfileId)
                .OrderByDescending(a => a.AppliedDate)
                .Select(a => new
                {
                    a.ApplicationId,
                    CompanyName = a.Company!.Name,
                    PackageLPA = a.Company.PackageLPA,
                    a.Status,
                    a.AppliedDate,
                    a.Remarks
                })
                .ToListAsync();

            return Ok(apps);
        }

        // ── PUT: api/Placement/status/{applicationId} ── Staff updates application status
        [Authorize(Roles = "Admin,Staff")]
        [HttpPut("status/{applicationId}")]
        public async Task<IActionResult> UpdateStatus(int applicationId, [FromBody] PlacementStatusDTO dto)
        {
            var validStatuses = new[] { "Shortlisted", "Selected", "Rejected" };
            if (!validStatuses.Contains(dto.Status))
                return BadRequest(new { message = "Status Shortlisted, Selected, athva Rejected j hovu joiye." });

            var app = await _context.PlacementApplications
                .Include(a => a.Company)
                .Include(a => a.StudentProfile)
                .FirstOrDefaultAsync(a => a.ApplicationId == applicationId);

            if (app == null)
                return NotFound(new { message = "Application maldi nahi." });

            app.Status = dto.Status;
            app.Remarks = dto.Remarks;

            // If selected, create a placement record
            if (dto.Status == "Selected")
            {
                var alreadyPlaced = await _context.PlacementRecords
                    .AnyAsync(pr => pr.StudentProfileId == app.StudentProfileId && pr.CompanyId == app.CompanyId);

                if (!alreadyPlaced)
                {
                    _context.PlacementRecords.Add(new PlacementRecord
                    {
                        StudentProfileId = app.StudentProfileId,
                        CompanyId = app.CompanyId,
                        PackageOffered = app.Company?.PackageLPA ?? 0,
                        PlacedDate = DateTime.UtcNow,
                        JobRole = string.Empty
                    });
                }
            }

            // Notify student
            var studentUserId = app.StudentProfile?.UserId ?? 0;
            if (studentUserId > 0)
            {
                _context.Notifications.Add(new Notification
                {
                    UserId = studentUserId,
                    Title = "Placement Update",
                    Message = $"Tamari '{app.Company?.Name}' company ni application {dto.Status} thai che.",
                    Type = "Placement",
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                });
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = $"Status {dto.Status} set thayo." });
        }

        // ── GET: api/Placement/applications/{companyId} ── Staff sees all applicants for a company
        [Authorize(Roles = "Admin,Staff,Faculty")]
        [HttpGet("applications/{companyId}")]
        public async Task<IActionResult> GetApplications(int companyId)
        {
            var apps = await _context.PlacementApplications
                .Include(a => a.StudentProfile).ThenInclude(sp => sp!.User)
                .Where(a => a.CompanyId == companyId)
                .OrderByDescending(a => a.AppliedDate)
                .Select(a => new
                {
                    a.ApplicationId,
                    StudentName = a.StudentProfile!.User!.Name,
                    EnrollmentNo = a.StudentProfile.EnrollmentNo,
                    a.Status,
                    a.AppliedDate,
                    a.Remarks
                })
                .ToListAsync();

            return Ok(apps);
        }

        // ── GET: api/Placement/placed-students ── All selected/placed students
        [Authorize(Roles = "Admin,Staff,Faculty")]
        [HttpGet("placed-students")]
        public async Task<IActionResult> GetPlacedStudents()
        {
            var placed = await _context.PlacementRecords
                .Include(pr => pr.StudentProfile).ThenInclude(sp => sp!.User)
                .Include(pr => pr.Company)
                .OrderByDescending(pr => pr.PlacedDate)
                .Select(pr => new
                {
                    pr.PlacementId,
                    StudentName = pr.StudentProfile!.User!.Name,
                    EnrollmentNo = pr.StudentProfile.EnrollmentNo,
                    CompanyName = pr.Company!.Name,
                    pr.PackageOffered,
                    pr.JobRole,
                    pr.PlacedDate
                })
                .ToListAsync();

            return Ok(placed);
        }
    }
}
