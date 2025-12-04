namespace Nexus.DEB.Application.Common.Models.Dms
{
    public class DmsDocumentHistoryList
    {
        public int Draw { get; set; }
        public int RecordsTotal { get; set; }
        public List<DmsDocumentHistoryItem> Data { get; set; } = new();
    }
}
