using Nexus.DEB.Domain.Interfaces;

namespace Nexus.DEB.Domain.Models
{
    public class ScopeSummary : IEntity, IOwnedBy
    {
        public Guid EntityId { get; set; }
        public string Title { get; set; }
        public Guid OwnedById { get; set; }
        public string? OwnedBy { get; set; }
        public int NumberOfLinkedStandardVersions { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime LastModifiedDate { get; set; }
        public string? Status { get; set; }

    }
}
