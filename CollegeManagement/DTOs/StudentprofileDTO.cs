using CollegeManagementSystem.Models;

namespace CollegeManagementSystem.DTOs
{
    public class StudentProfileDTO
    {
        public int UserId { get; set; }
        public string EnrollmentNo { get; set; } = string.Empty;
        public int Semester { get; set; }
        public int DepartmentId { get; set; }

        // Phase 7 — Commute Details
        public ResidenceType ResidenceType { get; set; } = ResidenceType.Local;
        public string HomeTown { get; set; } = string.Empty;
        public bool TransportOpted { get; set; } = false;
        public string? TransportRoute { get; set; }
    }
}