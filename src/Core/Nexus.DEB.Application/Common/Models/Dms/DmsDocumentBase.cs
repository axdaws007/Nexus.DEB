namespace Nexus.DEB.Application.Common.Models.Dms
{
    /// <summary>
    /// Base class containing common document properties shared across all libraries.
    /// </summary>
    public abstract class DmsDocumentBase
    {
        /// <summary>
        /// The unique document identifier.
        /// </summary>
        public Guid DocumentId { get; set; }

        /// <summary>
        /// The file name of the document.
        /// </summary>
        public string FileName { get; set; } = string.Empty;

        /// <summary>
        /// The file size in bytes.
        /// </summary>
        public int? FileSize { get; set; }

        /// <summary>
        /// The MIME type of the document.
        /// </summary>
        public string MimeType { get; set; } = string.Empty;

        /// <summary>
        /// The ID of the post that owns this document.
        /// </summary>
        public Guid DocumentOwnerId { get; set; }

        /// <summary>
        /// The display name of the document owner (post title).
        /// </summary>
        public string? DocumentOwner { get; set; }

        // Common metadata fields

        /// <summary>
        /// Document title.
        /// </summary>
        public string? Title { get; set; }

        /// <summary>
        /// Document description.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Document author.
        /// </summary>
        public string? Author { get; set; }

        /// <summary>
        /// Document type: "document" or "note".
        /// </summary>
        public string? DocumentType { get; set; }

        /// <summary>
        /// User who uploaded the document.
        /// </summary>
        public string? UploadedBy { get; set; }

        /// <summary>
        /// Date and time the document was uploaded.
        /// </summary>
        public DateTime? UploadedDate { get; set; }
    }
}
