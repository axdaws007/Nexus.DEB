namespace Nexus.DEB.Domain.Models
{
    public class StandardVersionSummary
    {
        public Guid StandardVersionId { get; set; }
        public string StandardTitle { get; set; }
        public string Version { get; set; }
        public string Title { get; set; }
        public DateTime EffectiveFrom { get; set; }
        public DateTime? EffectiveTo { get; set; }
        public DateTime LastModifiedDate { get; set; }
        public int NumberOfLinkedScopes { get; set; }

    }
}
