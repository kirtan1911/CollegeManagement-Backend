namespace CollegeManagementSystem.DTOs
{
    public class TimeTableEntryDTO
    {
        public int DepartmentId { get; set; }
        public int Semester { get; set; }
        public int CourseId { get; set; }
        /// <summary>Monday=1 … Saturday=6</summary>
        public int DayOfWeek { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string RoomNumber { get; set; } = string.Empty;
        public int FacultyUserId { get; set; }
    }
}
