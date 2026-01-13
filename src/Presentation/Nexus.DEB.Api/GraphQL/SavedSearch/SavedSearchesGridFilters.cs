namespace Nexus.DEB.Api.GraphQL
{
	public class SavedSearchesGridFilters
	{
		public string? SearchText { get; set; }
		public ICollection<string?>? Contexts { get; set; }
	}
}
