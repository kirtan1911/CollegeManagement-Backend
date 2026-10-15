namespace CollegeManagementSystem.Models
{
    public class TimeTableEntry
    {
        public int EntryId { get; set; }
        public int DepartmentId { get; set; }
        public int Semester { get; set; }
        public int CourseId { get; set; }
        /// <summary>Monday = 1 … Saturday = 6</summary>
        public int DayOfWeek { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string RoomNumber { get; set; } = string.Empty;
        public int FacultyUserId { get; set; }

        public Department? Department { get; set; }
        public Course? Course { get; set; }
    }
}

