namespace Nexus.DEB.Application.Common.Models.Dms
{
    /// <summary>
    /// Document model for the common-documents library.
    /// Contains common fields plus ExpiryDate and StandardVersionId.
    /// </summary>
    public class CommonDmsDocument : DmsDocumentBase
    {
        /// <summary>
        /// The expiry date of the document.
        /// </summary>
        public DateOnly? ExpiryDate { get; set; }

        /// <summary>
        /// The review date of the document.
        /// </summary>
        public DateOnly? ReviewDate { get; set; }

        /// <summary>
        /// Comma-delimited list of standard version IDs associated with this document.
        /// </summary>
        public string? StandardVersionId { get; set; }

        /// <summary>
        /// Gets the standard version IDs as a list of GUIDs.
        /// </summary>
        public List<Guid> GetStandardVersionIds()
        {
            if (string.IsNullOrWhiteSpace(StandardVersionId))
            {
                return new List<Guid>();
            }

            return StandardVersionId
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(s => Guid.TryParse(s.Trim(), out var guid) ? guid : (Guid?)null)
                .Where(g => g.HasValue)
                .Select(g => g!.Value)
                .ToList();
        }
    }
}
