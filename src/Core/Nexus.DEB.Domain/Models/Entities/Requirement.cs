using Nexus.DEB.Domain.Models.Common;

namespace Nexus.DEB.Domain.Models
{
    public class Requirement : EntityHead
    {
        public DateTime EffectiveStartDate { get; set; }
        public DateTime EffectiveEndDate { get; set; }
        public bool IsTitleDisplayed { get; set; }
        public bool IsReferenceDisplayed { get; set; }
        public short RequirementCategoryId { get; set; }
        public virtual RequirementCategory RequirementCategory { get; set; }
        public short RequirementTypeId { get; set; }
        public virtual RequirementType RequirementType { get; set; }
        public int? ComplianceWeighting { get; set; }
    }
}
