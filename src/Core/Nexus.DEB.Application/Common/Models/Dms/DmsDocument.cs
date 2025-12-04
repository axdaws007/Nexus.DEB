namespace Nexus.DEB.Application.Common.Models.Dms
{
    public class DmsDocument
    {
        public Guid DocumentId { get; set; }
        public Guid LibraryId { get; set; }
        public Guid? EntityId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string MimeType { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public int Version { get; set; }
        public DateTime UploadedDate { get; set; }
        public string? UploadedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string? ModifiedBy { get; set; }
    }
}
