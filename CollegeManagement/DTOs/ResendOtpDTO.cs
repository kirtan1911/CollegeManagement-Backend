using System.ComponentModel.DataAnnotations;

namespace CollegeManagementSystem.DTOs
{
    public class ResendOtpDTO
    {
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Please provide a valid email address.")]
        public string Email { get; set; } = string.Empty;
    }
}
