namespace Nexus.DEB.Application.Common.Models
{
    public class UserValidationResult
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
    }
}
