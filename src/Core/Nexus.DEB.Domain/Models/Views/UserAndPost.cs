namespace Nexus.DEB.Domain.Models
{
    public class UserAndPost
    {
        public Guid UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public bool IsUserDeleted { get; set; }
        public bool IsUserEnabled { get; set; }
        public Guid PostId { get; set; }
        public string PostTitle { get; set; } = string.Empty;
        public bool IsPostDeleted { get; set; }

        public string CombinedTitle { get => $"{PostTitle} ({UserName})"; }
    }
}
