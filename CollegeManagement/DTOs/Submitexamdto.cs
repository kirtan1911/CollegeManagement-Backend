namespace CollegeManagementSystem.DTOs
{
    public class SubmitExamDTO
    {
        public int AttemptId { get; set; }
        public List<AnswerDTO> Answers { get; set; } = new List<AnswerDTO>();
    }

    public class AnswerDTO
    {
        public int QuestionId { get; set; }
        public string SelectedOption { get; set; } = string.Empty; // "A" | "B" | "C" | "D"
    }
}