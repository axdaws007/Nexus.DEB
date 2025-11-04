namespace Nexus.DEB.Application.Common.Models
{
    public class SelectPostResponse
    {
        public Guid UserId { get; set; }
        public Guid PostId { get; set; }
        public bool Success { get; set; }
        public DateTimeOffset ExpiresAt { get; set; }
    }
}
