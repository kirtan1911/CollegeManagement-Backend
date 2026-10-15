namespace CollegeManagementSystem.Models
{
    public class Attendance
    {
        public int AttendanceId { get; set; }
        public int EnrollmentId { get; set; }
        public DateTime Date { get; set; }
        public bool IsPresent { get; set; }
        public int MarkedByUserId { get; set; } // Staff UserId

        public Enrollment? Enrollment { get; set; }
    }
}
