using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CollegeManagementSystem.Models
{
    public class PasswordResetOtp
    {
        [Key]                          
        public int OtpId { get; set; }

        public int UserId { get; set; }

        [Required]
        [MaxLength(6)]
        public string OtpCode { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public bool IsUsed { get; set; }

        /// <summary>Counts failed verify-otp attempts. OTP is invalidated after 5 failures.</summary>
        public int AttemptCount { get; set; } = 0;

        [ForeignKey("UserId")]
        public User? User { get; set; }
    }
}
