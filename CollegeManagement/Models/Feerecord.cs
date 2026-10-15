using System.ComponentModel.DataAnnotations;

namespace CollegeManagementSystem.Models
{
    public class FeeRecord
    {
        [Key]
        public int FeeId { get; set; }
        public int StudentProfileId { get; set; }
        public decimal Amount { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime? PaidDate { get; set; }
        public bool IsPaid { get; set; } = false;
        public string ReceiptNumber { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        public StudentProfile? StudentProfile { get; set; }
    }
}
