namespace Nexus.DEB.Application.Common.Models.Dms
{
    public class DmsDocumentHistoryItem
    {
        public int ID { get; set; }
        public DateTime DateModified { get; set; }
        public string? ModifiedBy { get; set; }
        public string? FieldChanges { get; set; }
        public DmsHistoryActionData ActionData { get; set; }
    }
}
