using System.ComponentModel.DataAnnotations;

namespace CollegeManagementSystem.Models
{
    public class ExamAnswer
    {
        [Key]
        public int AnswerId { get; set; }
        public int AttemptId { get; set; }
        public int QuestionId { get; set; }
        public string SelectedOption { get; set; } = string.Empty; // "A" | "B" | "C" | "D"
        public bool IsCorrect { get; set; }

        public ExamAttempt? Attempt { get; set; }
        public Question? Question { get; set; }
    }
}
