using Nexus.DEB.Domain.Interfaces;

namespace Nexus.DEB.Domain.Models
{
    public class StandardVersionSummary : IEntity
    {
        public Guid Id { get; set; }
        public string StandardTitle { get; set; }
        public string Version { get; set; }
        public string Title { get; set; }
        public DateTime EffectiveFrom { get; set; }
        public DateTime? EffectiveTo { get; set; }
        public DateTime LastModifiedDate { get; set; }
        public int NumberOfLinkedScopes { get; set; }

    }
}
