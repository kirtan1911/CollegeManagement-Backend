namespace CollegeManagementSystem.Models
{
    public enum ResidenceType
    {
        Local,
        Commuter,
        Hosteler,
        UpDown   // kept for backward compatibility with existing DB data
    }

    public class StudentProfile
    {
        public int StudentProfileId { get; set; }
        public int UserId { get; set; }
        public string EnrollmentNo { get; set; } = string.Empty;
        public int Semester { get; set; }
        public int DepartmentId { get; set; }

        // ── Phase 7: Commute Details ──
        public ResidenceType ResidenceType { get; set; } = ResidenceType.Local;
        public string HomeTown { get; set; } = string.Empty;
        public bool TransportOpted { get; set; } = false;
        /// <summary>Transport route — only relevant when TransportOpted is true.</summary>
        public string? TransportRoute { get; set; }

        public User? User { get; set; }
        public Department? Department { get; set; }

        public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
    }
}
