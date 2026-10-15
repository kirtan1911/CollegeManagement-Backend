using CollegeManagementSystem.Models;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace CollegeManagementSystem.DTOs
{
    public class UploadMaterialDTO
    {
        [Required]
        public int CourseId { get; set; }

        [Required]
        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        /// <summary>Legacy field kept for backwards compatibility.</summary>
        public string FileType { get; set; } = string.Empty;

        /// <summary>Category of material: Practical, LectureNote, PPT, GoogleClassroomLink, Image, Other.</summary>
        [Required]
        public MaterialCategory Category { get; set; } = MaterialCategory.Other;

        /// <summary>Required when Category == Practical. Must be > 0.</summary>
        public int? PracticalNumber { get; set; }

        /// <summary>Required when Category == GoogleClassroomLink. Must be a valid URL.</summary>
        public string? ExternalLink { get; set; }

        /// <summary>The uploaded file — optional when Category == GoogleClassroomLink.</summary>
        public IFormFile? File { get; set; }
    }
}