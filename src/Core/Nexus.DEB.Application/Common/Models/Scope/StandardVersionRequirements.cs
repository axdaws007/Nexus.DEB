namespace Nexus.DEB.Application.Common.Models
{
	public class StandardVersionRequirements
	{
		public Guid StandardVersionId { get; set; }
		public string StandardVersionTitle { get; set; } = string.Empty;
		public string Status { get; set; } = string.Empty;
		public List<Guid> RequirementIdsInScope { get; set; } = new List<Guid>();
		public int TotalRequirementsInScope { get; set; }
		public int TotalRequirements { get; set; }
	}
}
