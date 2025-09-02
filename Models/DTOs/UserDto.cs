namespace webapplication.Models.DTOs
{
    public class UserDto
    {
        public required string Id { get; set; }
        public string? FullName { get; set; }
        public required string Email { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public List<string> Roles { get; set; } = new List<string>();
    }
}
