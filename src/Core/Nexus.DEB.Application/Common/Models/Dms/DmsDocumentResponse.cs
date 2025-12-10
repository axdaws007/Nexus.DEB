namespace Nexus.DEB.Application.Common.Models.Dms
{
    public class DmsDocumentResponse
    {
        public bool Saved { get; set; }
        public string Title { get; set; }
        public bool FileAttachmentSaved { get; set; }
        public Guid? DocumentId { get; set; }
        public Decimal FileSizeKiloBytes { get; set; }
        public DateTime? LastModified { get; set; }
        public string ModifiedBy { get; set; }
    }
}
