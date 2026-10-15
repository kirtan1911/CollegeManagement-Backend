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
    public class StudyMaterialController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public StudyMaterialController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        private int GetUserId()
        {
            return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        }

        // POST: api/StudyMaterial/upload
        [Authorize(Roles = "Admin,Staff,Faculty")]
        [HttpPost("upload")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Upload([FromForm] UploadMaterialDTO dto)
        {
            var courseExists = await _context.Courses.AnyAsync(c => c.CourseId == dto.CourseId);
            if (!courseExists)
                return NotFound(new { message = "Subject maldo nahi." });

            // ── Validation per category ──

            if (dto.Category == MaterialCategory.GoogleClassroomLink)
            {
                if (string.IsNullOrWhiteSpace(dto.ExternalLink))
                    return BadRequest(new { message = "GoogleClassroomLink category mate ExternalLink required che." });
            }
            else
            {
                // All non-link categories require a file
                if (dto.File == null || dto.File.Length == 0)
                    return BadRequest(new { message = "File required che." });
            }

            if (dto.Category == MaterialCategory.Practical)
            {
                if (dto.PracticalNumber == null || dto.PracticalNumber <= 0)
                    return BadRequest(new { message = "Practical category mate PracticalNumber required che (Pr-1, Pr-2…)." });
            }

            string fileUrl = string.Empty;

            if (dto.Category != MaterialCategory.GoogleClassroomLink && dto.File != null)
            {
                // ── Security: file extension whitelist ──
                var allowedExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                {
                    ".pdf", ".doc", ".docx", ".ppt", ".pptx",
                    ".xls", ".xlsx", ".txt", ".jpg", ".jpeg",
                    ".png", ".gif", ".mp4", ".zip"
                };

                var originalExtension = Path.GetExtension(dto.File.FileName);
                if (!allowedExtensions.Contains(originalExtension))
                    return BadRequest(new { message = $"File type '{originalExtension}' not allowed. Allowed: pdf, doc, docx, ppt, pptx, xls, xlsx, txt, jpg, jpeg, png, gif, mp4, zip." });

                // ── Security: file size limit (10 MB) ──
                const long maxFileSizeBytes = 10 * 1024 * 1024;
                if (dto.File.Length > maxFileSizeBytes)
                    return BadRequest(new { message = "File size must not exceed 10 MB." });

                var webRoot = _env.WebRootPath ??
                              Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");

                var folderPath = Path.Combine(webRoot, "materials", dto.CourseId.ToString());

                if (!Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);

                // ── Security: use GUID-only filename to prevent path traversal ──
                var safeFileName = $"{Guid.NewGuid()}{originalExtension}";
                var filePath = Path.Combine(folderPath, safeFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                    await dto.File.CopyToAsync(stream);

                fileUrl = $"/materials/{dto.CourseId}/{safeFileName}";
            }

            var material = new StudyMaterial
            {
                CourseId = dto.CourseId,
                UploadedByUserId = GetUserId(),
                Title = dto.Title,
                Description = dto.Description,
                FileUrl = fileUrl,
                FileType = dto.FileType,
                Category = dto.Category,
                PracticalNumber = dto.Category == MaterialCategory.Practical ? dto.PracticalNumber : null,
                ExternalLink = dto.Category == MaterialCategory.GoogleClassroomLink ? dto.ExternalLink : null,
                UploadedAt = DateTime.UtcNow
            };

            _context.StudyMaterials.Add(material);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Material upload thayu!",
                material.MaterialId,
                material.FileUrl,
                material.ExternalLink,
                material.Category,
                material.PracticalNumber
            });
        }

        // DELETE: api/StudyMaterial/{materialId}
        [Authorize(Roles = "Admin,Staff,Faculty")]
        [HttpDelete("{materialId}")]
        public async Task<IActionResult> Delete(int materialId)
        {
            var material = await _context.StudyMaterials
                .FirstOrDefaultAsync(m => m.MaterialId == materialId);

            if (material == null)
                return NotFound(new { message = "Material maldo nahi." });

            // Delete file from disk if it's not a link-only material
            if (!string.IsNullOrEmpty(material.FileUrl))
            {
                var webRoot = _env.WebRootPath ??
                              Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");

                var filePath = Path.Combine(
                    webRoot,
                    material.FileUrl.TrimStart('/')
                        .Replace('/', Path.DirectorySeparatorChar));

                if (System.IO.File.Exists(filePath))
                    System.IO.File.Delete(filePath);
            }

            _context.StudyMaterials.Remove(material);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Material delete thayu." });
        }

        // GET: api/StudyMaterial/course/{courseId}?category=Practical
        [Authorize]
        [HttpGet("course/{courseId}")]
        public async Task<IActionResult> GetByCourse(int courseId, [FromQuery] MaterialCategory? category)
        {
            // ── Students may only access materials for enrolled courses ──
            if (User.IsInRole("Student"))
            {
                var userId = GetUserId();
                var profile = await _context.StudentProfiles
                    .FirstOrDefaultAsync(sp => sp.UserId == userId);

                if (profile == null)
                    return NotFound(new { message = "Student profile not found." });

                var isEnrolled = await _context.Enrollments
                    .AnyAsync(e => e.StudentProfileId == profile.StudentProfileId && e.CourseId == courseId);

                if (!isEnrolled)
                    return Forbid();
            }
            var query = _context.StudyMaterials
                .Where(m => m.CourseId == courseId)
                .AsQueryable();

            if (category.HasValue)
                query = query.Where(m => m.Category == category.Value);

            // Practicals sorted by PracticalNumber ASC; everything else by UploadedAt DESC
            List<object> materials;

            if (category == MaterialCategory.Practical)
            {
                materials = await query
                    .OrderBy(m => m.PracticalNumber)
                    .Select(m => (object)new
                    {
                        m.MaterialId,
                        m.Title,
                        m.Description,
                        m.FileUrl,
                        m.FileType,
                        m.Category,
                        CategoryName = m.Category.ToString(),
                        m.PracticalNumber,
                        m.ExternalLink,
                        m.UploadedAt
                    })
                    .ToListAsync();
            }
            else
            {
                materials = await query
                    .OrderByDescending(m => m.UploadedAt)
                    .Select(m => (object)new
                    {
                        m.MaterialId,
                        m.Title,
                        m.Description,
                        m.FileUrl,
                        m.FileType,
                        m.Category,
                        CategoryName = m.Category.ToString(),
                        m.PracticalNumber,
                        m.ExternalLink,
                        m.UploadedAt
                    })
                    .ToListAsync();
            }

            return Ok(materials);
        }
    }
}