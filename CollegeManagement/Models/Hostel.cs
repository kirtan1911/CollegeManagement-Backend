namespace CollegeManagementSystem.Models
{
    public class Hostel
    {
        public int HostelId { get; set; }
        public string Name { get; set; } = string.Empty;
        /// <summary>Boys / Girls / Mixed</summary>
        public string Type { get; set; } = string.Empty;
        public int TotalRooms { get; set; }

        public ICollection<HostelRoom> Rooms { get; set; } = new List<HostelRoom>();
    }

    public class HostelRoom
    {
        public int RoomId { get; set; }
        public int HostelId { get; set; }
        public string RoomNumber { get; set; } = string.Empty;
        public int Capacity { get; set; }
        public int OccupiedCount { get; set; }

        public Hostel? Hostel { get; set; }
        public ICollection<HostelAllotment> Allotments { get; set; } = new List<HostelAllotment>();
    }

    public class HostelAllotment
    {
        public int AllotmentId { get; set; }
        public int StudentProfileId { get; set; }
        public int RoomId { get; set; }
        public DateTime AllotmentDate { get; set; } = DateTime.UtcNow;
        public DateTime? VacatedDate { get; set; }
        /// <summary>Pending / Paid</summary>
        public string FeeStatus { get; set; } = "Pending";

        public StudentProfile? StudentProfile { get; set; }
        public HostelRoom? Room { get; set; }
    }
}

