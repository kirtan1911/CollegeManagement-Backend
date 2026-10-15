namespace CollegeManagementSystem.Models
{
    public class Exam
    {
        public int ExamId { get; set; }
        public string Title { get; set; } = string.Empty;
        public int CourseId { get; set; }
        public string ExamType { get; set; } = string.Empty; // "Internal1" | "Internal2" | "External"
        public decimal TotalMarks { get; set; }
        public string SecretCode { get; set; } = string.Empty;
        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
        public bool IsActive { get; set; } = true;
        public int CreatedByUserId { get; set; }

        public Course? Course { get; set; }
        public ICollection<Question> Questions { get; set; } = new List<Question>();
        public ICollection<ExamAttempt> Attempts { get; set; } = new List<ExamAttempt>();
    }
}
