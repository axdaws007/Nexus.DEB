namespace Nexus.DEB.Api.GraphQL.Authentication.Models
{
    public class SelectPostPayload
    {
        public Guid? UserId { get; set; }
        public Guid? PostId { get; set; }
        public bool Success { get; set; }
        public string? Message { get; set; }
        public DateTimeOffset? ExpiresAt { get; set; }
    }
}
