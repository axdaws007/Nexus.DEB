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
		public static async Task<SavedSearch> GetSavedSearchAsync(
			string context,
			string name,
			IDebService debService,
			CancellationToken cancellationToken)
			=> await debService.GetSavedSearchAsync(context, name, cancellationToken);
	}
}
