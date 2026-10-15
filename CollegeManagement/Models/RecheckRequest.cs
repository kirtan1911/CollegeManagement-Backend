namespace CollegeManagementSystem.Models
{
    public class RecheckRequest
    {
        public int RequestId { get; set; }
        public int StudentProfileId { get; set; }
        public int ExamId { get; set; }
        public DateTime RequestDate { get; set; } = DateTime.UtcNow;
        public string Reason { get; set; } = string.Empty;
        /// <summary>Pending / Approved / Rejected</summary>
        public string Status { get; set; } = "Pending";
        public decimal OldMarks { get; set; }
        public decimal? NewMarks { get; set; }
        public int? ProcessedByUserId { get; set; }
        public DateTime? ProcessedDate { get; set; }

        public StudentProfile? StudentProfile { get; set; }
        public Exam? Exam { get; set; }
    }
}

