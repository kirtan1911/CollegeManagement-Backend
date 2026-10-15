using System.ComponentModel.DataAnnotations;

namespace CollegeManagementSystem.Models
{
    /// <summary>
    /// Configurable grade scale table. Admin can edit entries without a code change.
    /// Default seed data: A+(≥90/10), A(80-89/9), B+(70-79/8), B(60-69/7), C+(50-59/6), C(40-49/5), Fail(&lt;40/0)
    /// </summary>
    public class GradeScale
    {
        [Key]
        public int GradeScaleId { get; set; }

        /// <summary>Minimum marks (inclusive) for this grade band.</summary>
        public decimal MinMarks { get; set; }

        /// <summary>Maximum marks (inclusive) for this grade band.</summary>
        public decimal MaxMarks { get; set; }

        /// <summary>Grade label e.g. "A+", "B", "Fail".</summary>
        public string Grade { get; set; } = string.Empty;

        /// <summary>Grade point on a 10-point scale.</summary>
        public decimal GradePoint { get; set; }
    }
}

