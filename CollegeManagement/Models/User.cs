namespace CollegeManagementSystem.Models
{
    public class User
    {
        public int UserId { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string PasswordHash { get; set; } = string.Empty;

        public int RoleId { get; set; }

        public bool Status { get; set; } = true;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public Role? Role { get; set; }
    }
}
