namespace Nexus.DEB.Api.GraphQL
{
	public class StandardVersionRequirementsFilters
	{
		public Guid StandardVersionId { get; set; }
		public Guid? SectionId { get; set; }
		public string? SearchText { get; set; }
		public Guid ScopeId { get; set; }
	}
}
