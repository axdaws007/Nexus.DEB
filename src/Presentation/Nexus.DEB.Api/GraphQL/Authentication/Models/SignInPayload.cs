namespace Nexus.DEB.Api.GraphQL.Authentication.Models
{
    /// <summary>
    /// Payload returned from sign-in mutation
    /// </summary>
    public class SignInPayload
    {
        public Guid? UserId { get; set; }
        public Guid? PostId { get; set; }
        public string? Username { get; set; }
        public bool Success { get; set; }
        public string? Message { get; set; }
        public DateTimeOffset? ExpiresAt { get; set; }
    }
}
