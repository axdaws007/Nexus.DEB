namespace Nexus.DEB.Domain.Models
{
    public class RequirementExport
    {
        public Guid EntityId { get; set; }
        public string? SerialNumber { get; set; }
        public string? SectionReferences { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime LastModifiedDate { get; set; }
        public int? StatusId { get; set; }
        public string? Status { get; set; }
        public DateTime EffectiveStartDate { get; set; }
        public DateTime EffectiveEndDate { get; set; }
        public string RequirementCategoryTitle { get; set; }
        public string RequirementTypeTitle { get; set; }
        public int? ComplianceWeighting { get; set; }
    }
}
