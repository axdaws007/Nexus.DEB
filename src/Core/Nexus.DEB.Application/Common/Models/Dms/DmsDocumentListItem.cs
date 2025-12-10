namespace Nexus.DEB.Application.Common.Models.Dms
{
    public class DmsDocumentListItem
    {
        public string? Title { get; set; }

        public string? Description { get; set; }

        public string? Author { get; set; }

        public string? FileType { get; set; }

        public string? UploadedBy { get; set; }

        public DateTime UploadedDate { get; set; }

        public string? UploadedDateFormatted { get; set; }

        public double FileSize { get; set; }

        public string? FileSizeFormatted { get; set; }

        public string? SerialNumber { get; set; }

        public DmsDocumentActionData? ActionData { get; set; }
    }
}
