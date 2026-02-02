namespace Nexus.DEB.Application.Common.Models.Dms
{
    public class DmsCommonDocumentListItem
    {
        public string? Title { get; set; }

        public string? Description { get; set; }

        public string? Author { get; set; }

        public string? DocumentType { get; set; }

        public string? UploadedBy { get; set; }

        public DateTime UploadedDate { get; set; }

        public DateTime? ExpiryDate { get; set; }

        public DateTime? ReviewDate { get; set; }

        public DmsDocumentActionData? ActionData { get; set; }
    }
}
