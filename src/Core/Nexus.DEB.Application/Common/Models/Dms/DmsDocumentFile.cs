namespace Nexus.DEB.Application.Common.Models.Dms
{
    public class DmsDocumentFile
    {
        public Guid DocumentId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string MimeType { get; set; } = string.Empty;
        public byte[] FileData { get; set; } = Array.Empty<byte>();
    }
}
