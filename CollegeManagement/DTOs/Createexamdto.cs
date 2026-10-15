namespace CollegeManagementSystem.DTOs
{
    public class CreateExamDTO
    {
        public string Title { get; set; } = string.Empty;
        public int CourseId { get; set; }
        public string ExamType { get; set; } = string.Empty; // "Internal1" | "Internal2" | "External"
        public decimal TotalMarks { get; set; }
        public string SecretCode { get; set; } = string.Empty;
        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
    }
}