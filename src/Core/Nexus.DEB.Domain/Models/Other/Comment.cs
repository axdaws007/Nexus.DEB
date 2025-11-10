namespace Nexus.DEB.Domain.Models
{
    public class Comment
    {
        public long Id { get; set; }
        public Guid EntityId { get; set; }
        public string Text { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public Guid? CreatedByPostId { get; set; }
        public Guid? CreatedByUserId { get; set; }
        public short CommentTypeId { get; set; }
        public virtual CommentType CommentType { get; set; }
        public string? CreatedByUserName { get; set; } = string.Empty;
        public string? CreatedByPostTitle { get; set; } = string.Empty;
    }
}
