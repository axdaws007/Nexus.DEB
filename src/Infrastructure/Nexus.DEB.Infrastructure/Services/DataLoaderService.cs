using Microsoft.EntityFrameworkCore;
using Nexus.DEB.Application.Common.Interfaces;
using Nexus.DEB.Domain;
using Nexus.DEB.Infrastructure.Persistence;

namespace Nexus.DEB.Infrastructure.Services
{
	public class DataLoaderService : IDataLoaderService
	{
		private readonly IDbContextFactory<DebContext> _factory;

		public DataLoaderService(IDbContextFactory<DebContext> factory)
		{
			_factory = factory;
		}

		public async Task<IReadOnlyDictionary<Guid, bool>> HasOtherDraftStandardVersionsForStandardsAsync(
			IEnumerable<Guid> entityIds,
			CancellationToken cancellationToken = default)
		{
			var entityIdList = entityIds.ToList();

			if (entityIdList.Count == 0)
				return new Dictionary<Guid, bool>();

			await using var _dbContext = await _factory.CreateDbContextAsync(cancellationToken);

			var query = from current in _dbContext.StandardVersions
						where entityIdList.Contains(current.EntityId)
						select new
						{
							current.EntityId,
							HasOtherActive = (
								from other in _dbContext.StandardVersions
								where other.StandardId == current.StandardId
							&& other.EntityId != current.EntityId
							&& !other.IsRemoved
								join ped in _dbContext.PawsEntityDetails on other.EntityId equals ped.EntityId
								where ped.PseudoStateTitle == DebHelper.Paws.States.Draft
								select other
							).Any()
						};

			return await query.ToDictionaryAsync(
				x => x.EntityId,
				x => x.HasOtherActive,
				cancellationToken);
		}

		public async Task<IReadOnlyDictionary<Guid, string?>> GetWorkflowPseudoStateTitleForEntitiesAsync(List<Guid> entityIds, CancellationToken cancellationToken = default)
		{
			await using var _dbContext = await _factory.CreateDbContextAsync(cancellationToken);

			return await _dbContext.PawsEntityDetails.AsNoTracking().Where(x => entityIds.Contains(x.EntityId)).ToDictionaryAsync(x => x.EntityId, x => x.PseudoStateTitle, cancellationToken);
		}
	}
}
