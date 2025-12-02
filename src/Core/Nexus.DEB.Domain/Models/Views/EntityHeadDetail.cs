namespace Nexus.DEB.Domain.Models
{
    public class EntityHeadDetail
    {
        public Guid EntityId { get; set; }
        public string Title { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public string? CreatedByPostTitle { get; set; }
        public DateTime LastModifiedDate { get; set; }
        public string? LastModifiedByPostTitle { get; set; }
        public string? OwnedByPostTitle { get; set; }
        public string? SerialNumber { get; set; }
    }
}
