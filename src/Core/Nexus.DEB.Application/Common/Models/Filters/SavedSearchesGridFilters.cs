namespace Nexus.DEB.Application.Common.Models
{
	public class SavedSearchesGridFilters
	{
		public string? SearchText { get; set; }
		public ICollection<string?>? Contexts { get; set; }
	}
}
