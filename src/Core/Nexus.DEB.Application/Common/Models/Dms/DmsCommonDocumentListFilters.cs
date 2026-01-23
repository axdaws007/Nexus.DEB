namespace Nexus.DEB.Application.Common.Models.Dms
{
    public class DmsCommonDocumentListFilters
    {
        public ICollection<Guid> StandardVersionIds { get; set; } = [];
        public string? SearchText { get; set; }
        public DateOnly? UploadedFrom { get; set; }
        public DateOnly? UploadedTo { get; set; }
        public string? Author { get; set; }
        public ICollection<string> DocumentTypes { get; set; } = [];
    }
}
