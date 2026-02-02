namespace Nexus.DEB.Application.Common.Models.Dms
{
    public class DmsApiDocumentResponse
    {
        public Guid DocumentId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public int? FileSize { get; set; }
        public string MimeType { get; set; } = string.Empty;
        public Guid DocumentOwnerId { get; set; }
        public string? DocumentOwner { get; set; }
        public Dictionary<string, object>? Metadata { get; set; }
    }
}
