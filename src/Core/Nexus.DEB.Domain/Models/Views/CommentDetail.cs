namespace Nexus.DEB.Domain.Models
{
    public class CommentDetail
    {
        public long Id { get; set; }
        public Guid EntityId { get; set; }
        public string Text { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public string? CreatedByPost { get; set; }
        public string? CreatedByFirstName { get; set; }
        public string? CreatedByLastName { get; set; }
        public string? CreatedByUserName { get; set; }
    }
}
