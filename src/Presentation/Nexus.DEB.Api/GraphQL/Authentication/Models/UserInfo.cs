namespace Nexus.DEB.Api.GraphQL.Authentication.Models
{
    /// <summary>
    /// User information returned after successful authentication
    /// </summary>
    public class UserInfo
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
        /// The username
        /// </summary>
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// When the authentication cookie expires
        /// </summary>
        public DateTime ExpiresAt { get; set; }
    }
}
