using System.ComponentModel.DataAnnotations;

namespace CollegeManagementSystem.DTOs
{
    public class RegisterDTO
    {
        [Required(ErrorMessage = "Name is required.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 100 characters.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Please provide a valid email address.")]
        [StringLength(254, ErrorMessage = "Email must not exceed 254 characters.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required.")]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters.")]
        [StringLength(128, ErrorMessage = "Password must not exceed 128 characters.")]
        public string Password { get; set; } = string.Empty;
    }
}