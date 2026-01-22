using Nexus.DEB.Application.Common.Interfaces;
using Nexus.DEB.Application.Common.Models.Filters;
using Nexus.DEB.Domain.Models.Common;

namespace Nexus.DEB.Api.GraphQL._Resolvers_
{
	public class FilterItemEntityTypeResolver
	{
		public async Task<int?> GetTotalRequirementsAsync([Parent] FilterItemEntity filterItemEntity, IDebService debService, CancellationToken cancellationToken)
		{
			if (!string.IsNullOrEmpty(filterItemEntity.EntityType))
			{
				switch(filterItemEntity.EntityType)
				{
					case EntityTypes.StandardVersion:
						var totalRequirements = await debService.GetStandardVersionTotalRequirementsAsync(filterItemEntity.Id, cancellationToken);
						return totalRequirements;
					default:
						return null;
				}
			}

			return null;
		}

		public async Task<string?> GetStatus([Parent] FilterItemEntity filterItemEntity, IDebService debService, CancellationToken cancellationToken)
		{
			if (!string.IsNullOrEmpty(filterItemEntity.EntityType))
			{
				switch (filterItemEntity.EntityType)
				{
					case EntityTypes.StandardVersion:
						var pawsState = await debService.GetWorkflowStatusByIdAsync(filterItemEntity.Id, cancellationToken);
						return pawsState.Status;
					default:
						return null;
				}
			}
			return null;
		}
	}
}
