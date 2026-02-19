namespace Nexus.DEB.Application.Common.Models
{
	public class RequirementDetail : EntityDetailBase
	{
		public short RequirementTypeId { get; set; }
		public string RequirementTypeTitle { get; set; } = string.Empty;
		public short RequirementCategoryId { get; set; }
		public string RequirementCategoryTitle { get; set; } = string.Empty;
		public DateOnly EffectiveStartDate { get; set; }
		public DateOnly EffectiveEndDate { get; set; }
		public int? ComplianceWeighting { get; set; }

        public ICollection<ScopeWithStatements> ScopeStatements { get; set; } = new List<ScopeWithStatements>();
		public ICollection<StandardVersionWithSections> StandardVersionSections { get; set; } = new List<StandardVersionWithSections>();
	}
}
