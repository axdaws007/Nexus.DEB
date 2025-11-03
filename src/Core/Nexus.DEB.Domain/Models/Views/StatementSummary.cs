using Nexus.DEB.Domain.Interfaces;

namespace Nexus.DEB.Domain.Models
{
    public class StatementSummary: IEntity, IOwnedBy
    {
        public Guid Id { get; set; }
        public string SerialNumber { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public DateTime LastModifiedDate { get; set; }
        public Guid OwnedById { get; set; }
        public string RequirementSerialNumbers { get; set; } = string.Empty;
    }
}
