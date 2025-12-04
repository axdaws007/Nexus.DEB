namespace Nexus.DEB.Application.Common.Models.Dms
{
    public class DmsDocumentResponse
    {
        public Guid DocumentId { get; set; }
        public string? FileName { get; set; }
        public long FileSize { get; set; }
        public string? Message { get; set; }
    }
}
