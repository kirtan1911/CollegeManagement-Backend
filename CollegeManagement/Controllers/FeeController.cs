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
    public class FeeController : ControllerBase
    {
        private readonly AppDbContext _context;

        public FeeController(AppDbContext context)
        {
            _context = context;
        }

        private int GetUserId()
        {
            return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        }

        // POST: api/Fee/add
        [Authorize(Roles = "Admin")]
        [HttpPost("add")]
        public async Task<IActionResult> AddFee(AddFeeDTO dto)
        {
            var profile = await _context.StudentProfiles
                .Include(sp => sp.User)
                .FirstOrDefaultAsync(sp => sp.StudentProfileId == dto.StudentProfileId);

            if (profile == null)
                return NotFound(new { message = "Student profile maldo nahi." });

            var fee = new FeeRecord
            {
                StudentProfileId = dto.StudentProfileId,
                Amount = dto.Amount,
                DueDate = dto.DueDate,
                Description = dto.Description,
                IsPaid = false,
                ReceiptNumber = string.Empty
            };

            _context.FeeRecords.Add(fee);
            // Add notification in the same unit-of-work (one SaveChangesAsync = atomic)
            var notification = new Notification
            {
                UserId = profile.UserId,
                Title = "Fee Due",
                Message = $"A fee of ₹{dto.Amount} is due by {dto.DueDate:dd-MM-yyyy}. ({dto.Description})",
                Type = "Fees",
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };
            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Fee record added successfully!", fee.FeeId });
        }

        // GET: api/Fee/all
        [Authorize(Roles = "Admin")]
        [HttpGet("all")]
        public async Task<IActionResult> GetAllFees()
        {
            var fees = await _context.FeeRecords
                .Include(f => f.StudentProfile)
                    .ThenInclude(sp => sp!.User)
                .OrderByDescending(f => f.DueDate)
                .Select(f => new
                {
                    f.FeeId,
                    f.StudentProfileId,
                    StudentName = f.StudentProfile!.User!.Name,
                    EnrollmentNo = f.StudentProfile.EnrollmentNo,
                    f.Amount,
                    f.DueDate,
                    f.PaidDate,
                    f.IsPaid,
                    f.ReceiptNumber,
                    f.Description
                })
                .ToListAsync();

            return Ok(fees);
        }

        // GET: api/Fee/student/{studentProfileId}
        [Authorize(Roles = "Admin")]
        [HttpGet("student/{studentProfileId}")]
        public async Task<IActionResult> GetByStudent(int studentProfileId)
        {
            var fees = await _context.FeeRecords
                .Where(f => f.StudentProfileId == studentProfileId)
                .OrderByDescending(f => f.DueDate)
                .ToListAsync();

            return Ok(fees);
        }

        // POST: api/Fee/pay/{feeId}
        [Authorize(Roles = "Student")]
        [HttpPost("pay/{feeId}")]
        public async Task<IActionResult> PayFee(int feeId)
        {
            var userId = GetUserId();

            var profile = await _context.StudentProfiles
                .FirstOrDefaultAsync(sp => sp.UserId == userId);

            if (profile == null)
                return NotFound(new { message = "Profile maldo nahi." });

            var fee = await _context.FeeRecords
                .FirstOrDefaultAsync(f => f.FeeId == feeId && f.StudentProfileId == profile.StudentProfileId);

            if (fee == null)
                return NotFound(new { message = "Fee record maldo nahi." });

            if (fee.IsPaid)
                return BadRequest(new { message = "Aa fee pehela thi j bharai gayel che." });

            fee.IsPaid = true;
            fee.PaidDate = DateTime.UtcNow;
            fee.ReceiptNumber = "RCP-" + DateTime.UtcNow.Year + "-" + fee.FeeId.ToString("D5");

            var notification = new Notification
            {
                UserId = userId,
                Title = "Fee Paid",
                Message = $"Tamari fee Rs.{fee.Amount} successfully bharai gayel che. Receipt No: {fee.ReceiptNumber}",
                Type = "Fees",
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };
            _context.Notifications.Add(notification);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Fee successfully bharai gayel che!",
                fee.ReceiptNumber,
                fee.PaidDate,
                fee.Amount
            });
        }

        // GET: api/Fee/my-fees
        [Authorize(Roles = "Student")]
        [HttpGet("my-fees")]
        public async Task<IActionResult> GetMyFees()
        {
            var userId = GetUserId();

            var profile = await _context.StudentProfiles
                .FirstOrDefaultAsync(sp => sp.UserId == userId);

            if (profile == null)
                return NotFound(new { message = "Profile maldo nahi." });

            var fees = await _context.FeeRecords
                .Where(f => f.StudentProfileId == profile.StudentProfileId)
                .OrderByDescending(f => f.DueDate)
                .Select(f => new
                {
                    f.FeeId,
                    f.Amount,
                    f.DueDate,
                    f.PaidDate,
                    f.IsPaid,
                    f.ReceiptNumber,
                    f.Description
                })
                .ToListAsync();

            return Ok(fees);
        }

        // GET: api/Fee/receipt/{feeId}
        [Authorize(Roles = "Student")]
        [HttpGet("receipt/{feeId}")]
        public async Task<IActionResult> GetReceipt(int feeId)
        {
            var userId = GetUserId();

            var profile = await _context.StudentProfiles
                .Include(sp => sp.User)
                .FirstOrDefaultAsync(sp => sp.UserId == userId);

            if (profile == null)
                return NotFound(new { message = "Profile maldo nahi." });

            var fee = await _context.FeeRecords
                .FirstOrDefaultAsync(f => f.FeeId == feeId && f.StudentProfileId == profile.StudentProfileId);

            if (fee == null)
                return NotFound(new { message = "Fee record maldo nahi." });

            if (!fee.IsPaid)
                return BadRequest(new { message = "Aa fee haju bharai nathi, receipt available nathi." });

            return Ok(new
            {
                fee.ReceiptNumber,
                StudentName = profile.User!.Name,
                EnrollmentNo = profile.EnrollmentNo,
                fee.Amount,
                fee.PaidDate,
                fee.Description,
                fee.DueDate
            });
        }
    }
}