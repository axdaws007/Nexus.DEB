namespace Nexus.DEB.Application.Common.Models.Dms
{
    public class DmsDocumentHistoryItem
    {
        public int Version { get; set; }
        public DateTime DateModified { get; set; }
        public string? ModifiedBy { get; set; }
        public string? Comments { get; set; }
        public long FileSize { get; set; }
    }
}
