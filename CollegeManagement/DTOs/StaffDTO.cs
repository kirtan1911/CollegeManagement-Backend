namespace CollegeManagementSystem.DTOs
{
    public class AttendanceDTO
    {
        public int EnrollmentId { get; set; }
        public DateTime Date { get; set; }
        public bool IsPresent { get; set; }
    }

    public class MarkDTO
    {
        public int EnrollmentId { get; set; }
        public string ExamType { get; set; } = string.Empty; // "Mid" or "Final"
        public decimal ObtainedMarks { get; set; }
        public decimal TotalMarks { get; set; }
    }
}