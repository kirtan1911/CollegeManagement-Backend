using BCrypt.Net;
using CollegeManagementSystem.Data;
using CollegeManagementSystem.DTOs;
using CollegeManagementSystem.Models;
using CollegeManagementSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace CollegeManagementSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly JwtService _jwtService;
        private readonly EmailService _emailService;

        public AuthController(AppDbContext context, JwtService jwtService, EmailService emailService)
        {
            _context = context;
            _jwtService = jwtService;
            _emailService = emailService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var emailExists = await _context.Users
                .AnyAsync(x => x.Email.ToLower() == dto.Email.ToLower());

            if (emailExists)
            {
                return BadRequest("Email already exists.");
            }

            var user = new User
            {
                Name = dto.Name,
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),

                // Default Role = Student
                RoleId = 3
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok("Registration Successful");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDTO dto)
        {
            var user = await _context.Users
                .Include(x => x.Role)
                .FirstOrDefaultAsync(x => x.Email == dto.Email);

            if (user == null)
            {
                return Unauthorized("Invalid Email or Password");
            }

            // Block deactivated accounts before password check
            if (!user.Status)
                return Unauthorized("Your account has been deactivated. Please contact the administrator.");

            bool isPasswordValid =
                BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash);

            if (!isPasswordValid)
            {
                return Unauthorized("Invalid Email or Password");
            }

            var token = _jwtService.GenerateToken(
                user,
                user.Role?.RoleName ?? "Student"
            );

            return Ok(new
            {
                Token = token,
                User = user.Name,
                Email = user.Email,
                Role = user.Role?.RoleName
            });
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("admin-dashboard")]
        public IActionResult AdminOnly()
        {
            return Ok("Admin Access");
        }

        [Authorize(Roles = "Staff")]
        [HttpGet("staff")]
        public IActionResult StaffOnly()
        {
            return Ok("Staff Access");
        }

        [Authorize(Roles = "Student")]
        [HttpGet("student")]
        public IActionResult StudentOnly()
        {
            return Ok("Student Access");
        }

        // ═══════════════════════════════════════════════════════════
        //  FORGOT PASSWORD — OTP FLOW
        //  These endpoints are completely separate from Login/Register.
        //  Login behaviour is unchanged.
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Step 1 — Generate and email a 6-digit OTP to the registered email.
        /// Always returns the same success message regardless of whether the
        /// email exists (prevents user enumeration).
        /// </summary>
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Look up user — do NOT reveal whether email exists
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == dto.Email && u.Status == true);

            // Generic response even when email is not found (security)
            if (user == null)
                return Ok(new { message = "If that email is registered, an OTP has been sent." });

            // Invalidate all previous unused OTPs for this user
            var existingOtps = await _context.PasswordResetOtps
                .Where(o => o.UserId == user.UserId && !o.IsUsed)
                .ToListAsync();

            foreach (var old in existingOtps)
                old.IsUsed = true;

            // Generate a new cryptographically secure 6-digit OTP
            var otpCode = GenerateOtp();
            var now = DateTime.UtcNow;

            var otpRecord = new PasswordResetOtp
            {
                UserId    = user.UserId,
                OtpCode   = otpCode,
                CreatedAt = now,
                ExpiresAt = now.AddMinutes(5),
                IsUsed    = false
            };

            _context.PasswordResetOtps.Add(otpRecord);
            await _context.SaveChangesAsync();

            // Send email — if sending fails, still return generic response
            // so the attacker cannot distinguish errors from a valid flow
            try
            {
                await _emailService.SendOtpEmailAsync(user.Email, user.Name, otpCode);
            }
            catch
            {
                // Log inside EmailService; swallow here to avoid info leak
            }

            return Ok(new { message = "If that email is registered, an OTP has been sent." });
        }

        /// <summary>
        /// Step 2 — Verify the OTP entered by the user.
        /// Used as a pre-check before showing the reset-password form.
        /// Does NOT mark the OTP as used — that happens on reset-password.
        /// </summary>
        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == dto.Email && u.Status == true);

            if (user == null)
                return BadRequest(new { message = "Invalid or expired OTP." });

            // Find the most recent unused OTP for this user
            var otp = await _context.PasswordResetOtps
                .Where(o => o.UserId == user.UserId && !o.IsUsed)
                .OrderByDescending(o => o.CreatedAt)
                .FirstOrDefaultAsync();

            if (otp == null)
                return BadRequest(new { message = "Invalid or expired OTP." });

            if (otp.ExpiresAt < DateTime.UtcNow)
                return BadRequest(new { message = "OTP has expired. Please request a new one." });

            // Increment attempt counter — max 5 wrong attempts before OTP is invalidated
            if (!otp.OtpCode.Equals(dto.OtpCode, StringComparison.Ordinal))
            {
                otp.AttemptCount++;
                if (otp.AttemptCount >= 5)
                {
                    otp.IsUsed = true; // invalidate after 5 wrong attempts
                    await _context.SaveChangesAsync();
                    return BadRequest(new { message = "Too many incorrect attempts. Please request a new OTP." });
                }
                await _context.SaveChangesAsync();
                return BadRequest(new { message = "Invalid or expired OTP." });
            }

            return Ok(new { message = "OTP verified successfully." });
        }

        /// <summary>
        /// Step 3 — Reset the password after OTP verification.
        /// Validates OTP, hashes the new password using BCrypt,
        /// updates PasswordHash in the Users table, and marks OTP as used.
        /// </summary>
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (dto.NewPassword != dto.ConfirmPassword)
                return BadRequest(new { message = "Passwords do not match." });

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == dto.Email && u.Status == true);

            if (user == null)
                return BadRequest(new { message = "Invalid or expired OTP." });

            // Re-validate OTP at reset step (prevents race condition if verify-otp was skipped)
            var otp = await _context.PasswordResetOtps
                .Where(o => o.UserId == user.UserId && !o.IsUsed)
                .OrderByDescending(o => o.CreatedAt)
                .FirstOrDefaultAsync();

            if (otp == null || !otp.OtpCode.Equals(dto.OtpCode, StringComparison.Ordinal))
                return BadRequest(new { message = "Invalid or expired OTP." });

            if (otp.ExpiresAt < DateTime.UtcNow)
                return BadRequest(new { message = "OTP has expired. Please request a new one." });

            // Mark OTP as used immediately (one-time use)
            otp.IsUsed = true;

            // Hash new password using existing BCrypt implementation
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);

            await _context.SaveChangesAsync();

            return Ok(new { message = "Password reset successfully. Please login with your new password." });
        }

        /// <summary>
        /// Resend — Invalidate all existing OTPs and generate a fresh one.
        /// Always returns the same generic message to prevent enumeration.
        /// </summary>
        [HttpPost("resend-otp")]
        public async Task<IActionResult> ResendOtp([FromBody] ResendOtpDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == dto.Email && u.Status == true);

            if (user == null)
                return Ok(new { message = "If that email is registered, a new OTP has been sent." });

            // Invalidate all previous OTPs
            var existingOtps = await _context.PasswordResetOtps
                .Where(o => o.UserId == user.UserId && !o.IsUsed)
                .ToListAsync();

            foreach (var old in existingOtps)
                old.IsUsed = true;

            // Generate fresh OTP
            var otpCode = GenerateOtp();
            var now = DateTime.UtcNow;

            var otpRecord = new PasswordResetOtp
            {
                UserId    = user.UserId,
                OtpCode   = otpCode,
                CreatedAt = now,
                ExpiresAt = now.AddMinutes(5),
                IsUsed    = false
            };

            _context.PasswordResetOtps.Add(otpRecord);
            await _context.SaveChangesAsync();

            try
            {
                await _emailService.SendOtpEmailAsync(user.Email, user.Name, otpCode);
            }
            catch
            {
                // Log inside EmailService; return generic message to avoid info leak
            }

            return Ok(new { message = "If that email is registered, a new OTP has been sent." });
        }

        // ── Private helper ────────────────────────────────────────────────

        /// <summary>Generates a cryptographically secure random 6-digit OTP string.</summary>
        private static string GenerateOtp()
        {
            // RandomNumberGenerator is cryptographically secure — not predictable like Random
            var bytes = new byte[4];
            RandomNumberGenerator.Fill(bytes);
            var value = BitConverter.ToUInt32(bytes, 0) % 900000 + 100000;
            return value.ToString();
        }
    }
}