namespace CollegeManagementSystem.Models
{
    public class Enrollment
    {
        public int EnrollmentId { get; set; }
        public int StudentProfileId { get; set; }
        public int CourseId { get; set; }
        public DateTime EnrolledDate { get; set; } = DateTime.UtcNow;

        public StudentProfile? StudentProfile { get; set; }
        public Course? Course { get; set; }

        public ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
        public ICollection<Mark> Marks { get; set; } = new List<Mark>();
    }
}
