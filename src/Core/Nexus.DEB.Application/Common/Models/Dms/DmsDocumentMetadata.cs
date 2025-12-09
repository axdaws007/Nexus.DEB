namespace Nexus.DEB.Application.Common.Models.Dms
{
    public class DmsDocumentMetadata
    {
        /// <summary>
        /// Document title
        /// </summary>
        public string? Title { get; set; }

        /// <summary>
        /// Document description
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Document author
        /// </summary>
        public string? Author { get; set; }

        /// <summary>
        /// Entity ID this document is associated with (REQUIRED by legacy API)
        /// </summary>
        public Guid EntityId { get; set; }

        /// <summary>
        /// Document type: "document" or "note" (defaults to "document")
        /// </summary>
        public string DocumentType { get; set; } = "document";
    }
}
