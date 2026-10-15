using CollegeManagementSystem.Models;
using System.ComponentModel.DataAnnotations;

namespace CollegeManagementSystem.DTOs
{
    /// <summary>
    /// DTO used by students to update their own commute and residence details
    /// via PUT /api/Student/profile.
    /// </summary>
    public class UpdateCommuteDTO
    {
        public ResidenceType ResidenceType { get; set; } = ResidenceType.Local;

        [StringLength(100, ErrorMessage = "HomeTown must not exceed 100 characters.")]
        public string? HomeTown { get; set; }

        public bool TransportOpted { get; set; } = false;

        [StringLength(200, ErrorMessage = "TransportRoute must not exceed 200 characters.")]
        public string? TransportRoute { get; set; }
    }
}
