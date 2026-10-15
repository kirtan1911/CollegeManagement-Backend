using System.ComponentModel.DataAnnotations;

namespace CollegeManagementSystem.Models
{
    public enum MaterialCategory
    {
        Practical,
        LectureNote,
        PPT,
        GoogleClassroomLink,
        Image,
        Other
    }

    public class StudyMaterial
    {
        [Key]
        public int MaterialId { get; set; }

        public int CourseId { get; set; }

        public int UploadedByUserId { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        /// <summary>Relative URL of the uploaded file. Empty string if Category == GoogleClassroomLink.</summary>
        public string FileUrl { get; set; } = string.Empty;

        /// <summary>Legacy type string (PDF, Image etc.) — kept for backward compat.</summary>
        public string FileType { get; set; } = string.Empty;

        public MaterialCategory Category { get; set; } = MaterialCategory.Other;

        /// <summary>Required only when Category == Practical (Pr-1, Pr-2…). Null otherwise.</summary>
        public int? PracticalNumber { get; set; }

        /// <summary>External URL — used when Category == GoogleClassroomLink.</summary>
        public string? ExternalLink { get; set; }

        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        public Course? Course { get; set; }
    }
}
