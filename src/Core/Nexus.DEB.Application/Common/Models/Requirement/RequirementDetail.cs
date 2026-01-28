namespace Nexus.DEB.Application.Common.Models
{
	public class RequirementDetail : EntityDetailBase
	{
		public int RequirementTypeId { get; set; }
		public string RequirementTypeTitle { get; set; } = string.Empty;
		public int RequirementCategoryId { get; set; }
		public string RequirementCategoryTitle { get; set; } = string.Empty;

		public ICollection<ScopeWithStatements> ScopeStatements { get; set; } = new List<ScopeWithStatements>();
		public ICollection<StandardVersionWithSections> StandardVersionSections { get; set; } = new List<StandardVersionWithSections>();
	}
}
