using Nexus.DEB.Domain.Interfaces;

namespace Nexus.DEB.Domain.Models
{
    public class StandardVersionSummary : IEntity, IEntityType
    {
        public Guid EntityId { get; set; }
        public short StandardId { get; set; }
        public string StandardTitle { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public string? Title { get; set; }
        public DateOnly EffectiveFrom { get; set; }
        public DateOnly? EffectiveTo { get; set; }
        public DateTime LastModifiedDate { get; set; }
        public int NumberOfLinkedScopes { get; set; }
        public int StatusId { get; set; }
        public string? Status { get; set; }
        public string EntityTypeTitle { get; set; }

    }
}
