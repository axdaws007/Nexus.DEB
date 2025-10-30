namespace Nexus.DEB.Api.GraphQL.Authentication.Models
{
    public class SignInInput
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public bool RememberMe { get; set; } = false;
    }
}
