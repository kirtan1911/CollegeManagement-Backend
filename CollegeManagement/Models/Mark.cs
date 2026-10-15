namespace CollegeManagementSystem.Models
{
    public class Mark
    {
        public int MarkId { get; set; }
        public int EnrollmentId { get; set; }
        public string ExamType { get; set; } = string.Empty; // "Mid", "Final"
        public decimal ObtainedMarks { get; set; }
        public decimal TotalMarks { get; set; }
        public int AddedByUserId { get; set; } // Staff UserId

        public Enrollment? Enrollment { get; set; }
    }
}
