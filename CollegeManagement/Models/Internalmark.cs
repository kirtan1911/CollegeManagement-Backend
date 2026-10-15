namespace CollegeManagementSystem.Models
{
    public class InternalMark
    {
        public int InternalMarkId { get; set; }
        public int StudentProfileId { get; set; }
        public int CourseId { get; set; }

        /// <summary>Score from Internal-1 exam (max 20). Auto-populated by ExamController.SubmitExam.</summary>
        public decimal Internal1Score { get; set; }   // max 20

        /// <summary>Score from Internal-2 exam (max 20). Auto-populated by ExamController.SubmitExam.</summary>
        public decimal Internal2Score { get; set; }   // max 20

        /// <summary>Best of Internal1Score / Internal2Score. Computed on every SubmitExam.</summary>
        public decimal BestInternalScore { get; set; } // max 20

        /// <summary>Attendance-based marks (0–5). Auto-suggested from attendance %, editable by faculty.</summary>
        public decimal AttendanceMarks { get; set; }   // max 5

        /// <summary>Quiz marks (0–5). Entered by faculty.</summary>
        public decimal QuizMarks { get; set; }         // max 5

        /// <summary>
        /// Faculty-discretion marks (0–10): covers viva, notes, assignments etc.
        /// Replaces the old LectureNoteMarks (0–5) + VivaMarks (0–5).
        /// </summary>
        public decimal FacultyDiscretionMarks { get; set; } // max 10

        /// <summary>
        /// Total internal marks = BestInternalScore + AttendanceMarks + QuizMarks + FacultyDiscretionMarks.
        /// Max 40.
        /// </summary>
        public decimal TotalInternal { get; set; }     // max 40

        public StudentProfile? StudentProfile { get; set; }
        public Course? Course { get; set; }
    }
}
