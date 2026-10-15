namespace CollegeManagementSystem.DTOs
{
    public class AddQuestionDTO
    {
        public int ExamId { get; set; }
        public string QuestionText { get; set; } = string.Empty;
        public string OptionA { get; set; } = string.Empty;
        public string OptionB { get; set; } = string.Empty;
        public string OptionC { get; set; } = string.Empty;
        public string OptionD { get; set; } = string.Empty;
        public string CorrectOption { get; set; } = string.Empty; // "A" | "B" | "C" | "D"
        public decimal Marks { get; set; }
    }
}