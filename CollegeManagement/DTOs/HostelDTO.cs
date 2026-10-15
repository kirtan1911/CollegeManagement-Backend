namespace CollegeManagementSystem.DTOs
{
    public class HostelDTO
    {
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty; // Boys / Girls / Mixed
        public int TotalRooms { get; set; }
    }

    public class HostelRoomDTO
    {
        public int HostelId { get; set; }
        public string RoomNumber { get; set; } = string.Empty;
        public int Capacity { get; set; }
    }

    public class HostelAllotmentDTO
    {
        public int StudentProfileId { get; set; }
        public int RoomId { get; set; }
    }
}
