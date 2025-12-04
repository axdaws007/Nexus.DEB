using HotChocolate.Authorization;
using Nexus.DEB.Application.Common.Interfaces;
using Nexus.DEB.Domain.Models;

namespace Nexus.DEB.Api.GraphQL
{
	[MutationType]
	public static class SavedSearchMutations
	{
		[Authorize]
		public static async Task<SavedSearch?> SaveSavedSearchAsync(
			string context,
			string name,
			string filter,
			ISavedSearchDomainService savedSearchService,
			CancellationToken cancellationToken = default)
		{
			var result = await savedSearchService.SaveSavedSearchAsync(context, name, filter, cancellationToken);

			if (!result.IsSuccess)
			{
				throw ExceptionHelper.BuildException(result);
			}

			return result.Data;
		}
	}
}
