namespace CollegeManagementSystem.DTOs
{
    public class UpdateInternalDTO
    {
        public int StudentProfileId { get; set; }
        public int CourseId { get; set; }

        /// <summary>Attendance marks (0–5). Faculty can override the auto-suggested value.</summary>
        public decimal AttendanceMarks { get; set; }   // 0-5

        /// <summary>Quiz marks (0–5).</summary>
        public decimal QuizMarks { get; set; }         // 0-5

        /// <summary>
        /// Faculty-discretion marks (0–10): viva, notes, assignments, etc.
        /// Replaces the old LectureNoteMarks + VivaMarks combination.
        /// </summary>
        public decimal FacultyDiscretionMarks { get; set; } // 0-10
    }
}