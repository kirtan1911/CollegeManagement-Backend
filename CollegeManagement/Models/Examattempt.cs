using System.ComponentModel.DataAnnotations;

namespace CollegeManagementSystem.Models
{
    public class ExamAttempt
    {
        [Key]
        public int AttemptId { get; set; }
        public int ExamId { get; set; }
        public int StudentProfileId { get; set; }
        public DateTime StartedAt { get; set; } = DateTime.UtcNow;
        public DateTime? SubmittedAt { get; set; }
        public decimal TotalObtained { get; set; }
        public bool IsSubmitted { get; set; } = false;

        public Exam? Exam { get; set; }
        public StudentProfile? StudentProfile { get; set; }
        public ICollection<ExamAnswer> Answers { get; set; } = new List<ExamAnswer>();
    }
}
