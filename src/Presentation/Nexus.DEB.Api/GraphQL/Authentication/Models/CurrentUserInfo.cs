namespace Nexus.DEB.Api.GraphQL.Authentication.Models
{
    public class CurrentUserInfo
    {
        /// <summary>
        /// The user's Post ID
        /// </summary>
        public Guid PostId { get; set; }

        /// <summary>
        /// The user's User ID
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// The username (in format "PostId|UserId")
        /// </summary>
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// Whether the user is authenticated
        /// </summary>
        public bool IsAuthenticated { get; set; }
    }
}
