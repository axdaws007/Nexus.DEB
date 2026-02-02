namespace Nexus.DEB.Application.Common.Models.Dms
{
    /// <summary>
    /// Document model for the deb-documents library.
    /// Contains common fields plus EntityId.
    /// </summary>
    public class DebDmsDocument : DmsDocumentBase
    {
        /// <summary>
        /// The entity ID this document is associated with.
        /// </summary>
        public Guid? EntityId { get; set; }
    }
}
