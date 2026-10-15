using System.ComponentModel.DataAnnotations;

namespace CollegeManagementSystem.DTOs
{
    public class VerifyOtpDTO
    {
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Please provide a valid email address.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "OTP is required.")]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "OTP must be exactly 6 digits.")]
        public string OtpCode { get; set; } = string.Empty;
    }
}
