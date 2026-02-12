namespace Nexus.DEB.Domain.Models
{
    public enum EntityDocumentLinkingContexts
    {
        CommonEvidence = 1
    }

    public class EntityDocumentLinking
    {
        public long Id { get; set; }
        public Guid EntityId { get; set; }
        public Guid LibraryId { get; set; }
        public Guid DocumentId { get; set; }
        public EntityDocumentLinkingContexts Context { get; set; }
    }
}
