using HotChocolate.Authorization;
using Nexus.DEB.Application.Common.Interfaces;
using Nexus.DEB.Domain.Models;

namespace Nexus.DEB.Api.GraphQL
{
	[QueryType]
	public static class SavedSearchQueries
	{
		[Authorize]
		public static async Task<ICollection<SavedSearch>> GetSavedSearchesByContextAsync(
			string context,
			IDebService debService,
			CancellationToken cancellationToken)
			=> await debService.GetSavedSearchesByContextAsync(context, cancellationToken);

		[Authorize]
		[UseOffsetPaging]
		[UseSorting]
		public static IQueryable<SavedSearch> GetSavedSearchesForGrid(
			SavedSearchesGridFilters? filters,
			IDebService debService,
			CancellationToken cancellationToken)
		{
            var f = filters is null
			? new Application.Common.Models.SavedSearchesGridFilters()
			: new Application.Common.Models.SavedSearchesGridFilters
			{
				SearchText = filters.SearchText?.Trim(),
				Contexts = filters.Contexts,
			};

			return debService.GetSavedSearchesForGridAsync(f, cancellationToken);
        }

		[Authorize]
		public static async Task<ICollection<ContextFilterItem>> GetSavedSearchContextsLookupAsync(
			IDebService debService,
			CancellationToken cancellationToken)
		{
			var contexts = await debService.GetSavedSearchContextsAsync(cancellationToken);

			return contexts
				.Select(s => new ContextFilterItem
				{
					Id = s,
					Value = s,
					IsEnabled = true
				}).ToList();
		}

		[Authorize]
		public static async Task<SavedSearch> GetSavedSearchAsync(
			string context,
			string name,
			IDebService debService,
			CancellationToken cancellationToken)
			=> await debService.GetSavedSearchAsync(context, name, cancellationToken);
	}
}
