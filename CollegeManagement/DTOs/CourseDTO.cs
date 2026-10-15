namespace CollegeManagementSystem.DTOs
{
    public class CourseDTO
    {
        public string CourseName { get; set; } = string.Empty;
        public string CourseCode { get; set; } = string.Empty;
        public int Credits { get; set; }
        public int DepartmentId { get; set; }
        /// <summary>Semester (1–8) that this subject belongs to.</summary>
        public int Semester { get; set; }
    }
}