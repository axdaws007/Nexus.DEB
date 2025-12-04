namespace Nexus.DEB.Application.Common.Models.Dms
{
    public class DmsDocumentList
    {
        public int Draw { get; set; }
        public int RecordsTotal { get; set; }
        public Guid LibraryId { get; set; }
        public List<DmsDocumentListItem> Data { get; set; } = new();
    }
}
