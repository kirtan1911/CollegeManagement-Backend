namespace CollegeManagementSystem.Models
{
    public class Company
    {
        public int CompanyId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Industry { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        /// <summary>Free-text eligibility criteria, e.g. "Min CGPA 6.0, No backlog".</summary>
        public string EligibilityCriteria { get; set; } = string.Empty;
        public decimal PackageLPA { get; set; }
        public DateTime DriveDate { get; set; }
        public bool IsActive { get; set; } = true;
        public string Description { get; set; } = string.Empty;

        public ICollection<PlacementApplication> Applications { get; set; } = new List<PlacementApplication>();
    }

    public class PlacementApplication
    {
        public int ApplicationId { get; set; }
        public int StudentProfileId { get; set; }
        public int CompanyId { get; set; }
        public DateTime AppliedDate { get; set; } = DateTime.UtcNow;
        /// <summary>Applied / Shortlisted / Selected / Rejected</summary>
        public string Status { get; set; } = "Applied";
        public string? Remarks { get; set; }

        public StudentProfile? StudentProfile { get; set; }
        public Company? Company { get; set; }
    }

    public class PlacementRecord
    {
        public int PlacementId { get; set; }
        public int StudentProfileId { get; set; }
        public int CompanyId { get; set; }
        public decimal PackageOffered { get; set; }
        public DateTime PlacedDate { get; set; } = DateTime.UtcNow;
        public string JobRole { get; set; } = string.Empty;

        public StudentProfile? StudentProfile { get; set; }
        public Company? Company { get; set; }
    }
}

