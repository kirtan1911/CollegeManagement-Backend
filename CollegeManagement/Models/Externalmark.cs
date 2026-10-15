namespace CollegeManagementSystem.Models
{
    public class ExternalMark
    {
        public int ExternalMarkId { get; set; }
        public int StudentProfileId { get; set; }
        public int CourseId { get; set; }
        public decimal ExternalScore { get; set; } // max 60

        public StudentProfile? StudentProfile { get; set; }
        public Course? Course { get; set; }
    }
}
