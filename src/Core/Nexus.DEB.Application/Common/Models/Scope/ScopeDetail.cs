namespace Nexus.DEB.Application.Common.Models
{
	public class ScopeDetail : EntityDetailBase
	{
		public DateOnly? TargetImplementationDate { get; set; }

		public List<StandardVersionRequirements> StandardVersionRequirements { get; set; } = new();

		public List<Guid> RequirementIds { get; set; } = new();
	}
}
