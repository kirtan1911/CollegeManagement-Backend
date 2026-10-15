namespace CollegeManagementSystem.DTOs
{
    public class RecheckRequestDTO
    {
        public int ExamId { get; set; }
        public string Reason { get; set; } = string.Empty;
    }

    public class ProcessRecheckDTO
    {
        public string Status { get; set; } = string.Empty; // Approved / Rejected
        public decimal? NewMarks { get; set; }
        public string? Remarks { get; set; }
    }
}
