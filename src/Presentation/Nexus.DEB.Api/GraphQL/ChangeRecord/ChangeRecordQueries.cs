using HotChocolate.Authorization;
using Nexus.DEB.Application.Common.Interfaces;
using Nexus.DEB.Application.Common.Models;
using Nexus.DEB.Domain.Models;

namespace Nexus.DEB.Api.GraphQL
{
	[QueryType]
	public static class ChangeRecordQueries
	{
		[Authorize]
		public static async Task<ICollection<ChangeRecord>> GetChangeRecordsForEntityAsync(
			Guid entityId,
			IDebService debService,
			CancellationToken cancellationToken)
			=> await debService.GetChangeRecordsForEntityAsync(entityId, cancellationToken);

		[Authorize]
		public static async Task<ICollection<ChangeRecordItemModel>> GetChangeRecordItemsForChangeRecordAsync(
			long changeRecordId,
			IDebService debService,
			CancellationToken cancellationToken)
			=> await debService.GetChangeRecordItemsForChangeRecordAsync(changeRecordId, cancellationToken);
	}
}
