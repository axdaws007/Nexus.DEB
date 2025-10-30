namespace Nexus.DEB.Api.GraphQL.Authentication.Models
{
    public class SignOutPayload
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
