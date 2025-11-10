namespace Nexus.DEB.Domain.Models
{
    public class StatementExport
    {
        public Guid EntityId { get; set; }
        public string SerialNumber { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? Description{ get; set; } 
        public DateTime LastModifiedDate { get; set; }
        public Guid OwnedById { get; set; }
        public string? OwnedBy { get; set; }
        public string RequirementSerialNumbers { get; set; } = string.Empty;
        public int? StatusId { get; set; }
        public string? Status { get; set; }
    }
}
