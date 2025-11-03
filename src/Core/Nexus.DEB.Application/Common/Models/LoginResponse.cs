namespace Nexus.DEB.Application.Common.Models
{
    public class LoginResponse
    {
        public Guid UserId { get; set; }
        public Guid PostId { get; set; }
        public string Username { get; set; } = string.Empty;
        public bool Success { get; set; }
        public DateTimeOffset ExpiresAt { get; set; }
        public List<CisPost>? Posts { get; set; }
        public string PostTitle { get; set; } = string.Empty;
    }
}
