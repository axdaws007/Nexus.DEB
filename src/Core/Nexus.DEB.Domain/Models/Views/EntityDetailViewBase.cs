namespace Nexus.DEB.Domain.Models
{
    public abstract class EntityDetailViewBase
    {
        public Guid EntityId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? SerialNumber { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime LastModifiedDate { get; set; }
        public bool IsRemoved { get; set; }
        public bool IsArchived { get; set; }
        public string EntityTypeTitle { get; set; } = string.Empty;
        public string? CreatedBy { get; set; }
        public string? LastModifiedBy { get; set; }
        public string? OwnedBy { set; get; }
        public Guid OwnedById { set; get; }
    }
}
