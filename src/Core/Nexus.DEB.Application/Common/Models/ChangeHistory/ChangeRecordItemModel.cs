namespace Nexus.DEB.Application.Common.Models
{
    public class ChangeRecordItemModel
    {
        public int Id { get; set; }
        public int ChangeRecordId { get; set; }
        public string FieldName { get; set; } = string.Empty;
        public string FriendlyFieldName { get; set; } = string.Empty;
        public string ChangedFrom { get; set; } = string.Empty;
        public string ChangedTo { get; set; } = string.Empty;
    }
}
