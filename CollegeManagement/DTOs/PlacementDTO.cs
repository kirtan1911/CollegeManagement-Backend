namespace CollegeManagementSystem.DTOs
{
    public class CompanyDTO
    {
        public string Name { get; set; } = string.Empty;
        public string Industry { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string EligibilityCriteria { get; set; } = string.Empty;
        public decimal PackageLPA { get; set; }
        public DateTime DriveDate { get; set; }
        public string Description { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
    }

    public class PlacementStatusDTO
    {
        public string Status { get; set; } = string.Empty; // Shortlisted / Selected / Rejected
        public string? Remarks { get; set; }
    }
}
