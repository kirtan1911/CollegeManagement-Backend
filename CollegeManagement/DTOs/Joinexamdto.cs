namespace CollegeManagementSystem.DTOs
{
    public class JoinExamDTO
    {
        public int ExamId { get; set; }
        public string SecretCode { get; set; } = string.Empty;
        public string EnrollmentNo { get; set; } = string.Empty;
    }
}