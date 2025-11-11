using Nexus.DEB.Domain.Interfaces;

namespace Nexus.DEB.Application.Common.Models
{

    public class RequirementSummary : IEntity
    {
        public Guid EntityId { get; set; }
        public string? SerialNumber { get; set; }
        public ICollection<SectionItem>? Sections { get; set; }
        public string Title { get; set; } = string.Empty;
        public DateTime LastModifiedDate { get; set; }
        public int? StatusId { get; set; }
        public string? Status { get; set; }
    }
}
