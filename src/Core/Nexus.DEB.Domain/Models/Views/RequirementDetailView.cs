namespace Nexus.DEB.Domain.Models
{
    public class RequirementDetailView : EntityDetailViewBase
    {
        public DateOnly EffectiveStartDate { get; set; }
        public DateOnly EffectiveEndDate { get; set; }
        public bool IsTitleDisplayed { get; set; }
        public bool IsReferenceDisplayed { get; set; }
        public short? RequirementCategoryId { get; set; }
        public short? RequirementTypeId { get; set; }
        public int? ComplianceWeighting { get; set; }
        public virtual RequirementCategory? RequirementCategory { get; set; }
        public virtual RequirementType? RequirementType { get; set; }
    }
}
