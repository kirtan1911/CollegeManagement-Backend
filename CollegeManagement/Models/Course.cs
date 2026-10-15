namespace CollegeManagementSystem.Models
{
    public class Course
    {
        public int CourseId { get; set; }

        public string CourseName { get; set; } = string.Empty;

        public string CourseCode { get; set; } = string.Empty;

        public int Credits { get; set; }

        /// <summary>Semester number (1–8). Scopes this subject to a specific semester within the department.</summary>
        public int Semester { get; set; }

        public int DepartmentId { get; set; }

        public Department? Department { get; set; }

        public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
    }
}
