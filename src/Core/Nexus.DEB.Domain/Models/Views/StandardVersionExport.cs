namespace Nexus.DEB.Domain.Models
{
    public class StandardVersionExport
    {
        public Guid EntityId { get; set; }
        public short StandardId { get; set; }
        public string StandardTitle { get; set; } = string.Empty;
        public string? SerialNumber { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime EffectiveStartDate { get; set; }
        public DateTime? EffectiveEndDate { get; set; }
        public DateTime LastModifiedDate { get; set; }
        public string Delimiter { get; set; } = string.Empty;
        public int? MajorVersion { get; set; }
        public int? MinorVersion { get; set; }
        public int StatusId { get; set; }
        public string? Status {  get; set; }
        public int NumberOfLinkedScopes { get; set; }
        public string VersionTitle { get; set; } = string.Empty;
    }
}
