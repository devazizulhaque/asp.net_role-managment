namespace webapplication.Models.DTOs
{
    public class RefreshTokenPayload
    {
        public string UserId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public long Expiry { get; set; }
    }
}
