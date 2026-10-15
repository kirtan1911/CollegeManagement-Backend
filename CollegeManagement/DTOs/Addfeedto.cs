namespace CollegeManagementSystem.DTOs
{
    public class AddFeeDTO
    {
        public int StudentProfileId { get; set; }
        public decimal Amount { get; set; }
        public DateTime DueDate { get; set; }
        public string Description { get; set; } = string.Empty;
    }
}