using Nexus.DEB.Domain.Interfaces;

namespace Nexus.DEB.Domain.Models
{
    public class StandardVersionSummary : IEntity
    {
        public Guid EntityId { get; set; }
        public short StandardId { get; set; }
        public string StandardTitle { get; set; }
        public string Version { get; set; }
        public string Title { get; set; }
        public DateTime EffectiveFrom { get; set; }
        public DateTime? EffectiveTo { get; set; }
        public DateTime LastModifiedDate { get; set; }
        public int NumberOfLinkedScopes { get; set; }
        public int? StatusId { get; set; }
        public string? Status { get; set; }

    }
}
