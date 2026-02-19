using HotChocolate.Authorization;
using Nexus.DEB.Application.Common.Interfaces;
using Nexus.DEB.Application.Common.Models;
using Nexus.DEB.Application.Common.Models.Filters;
using Nexus.DEB.Domain.Models;

namespace Nexus.DEB.Api.GraphQL
{
    [QueryType]
    public static class RequirementQueries
    {
        [Authorize]
        [UseOffsetPaging]
        [UseSorting]
        public static IQueryable<RequirementSummary> GetRequirementsForGrid(
            RequirementSummaryFilters? filters,
            IDebService debService)
			=> debService.GetRequirementsForGrid(filters);

		[Authorize]
		[UseOffsetPaging]
		[UseSorting]
		public static async Task<IEnumerable<StandardVersionRequirementDetail>> GetStandardVersionRequirementsForGrid(
			StandardVersionRequirementsFilters? filters,
			IDebService debService, 
            CancellationToken cancellationToken)
		{
			var f = filters is null
			? new Application.Common.Models.StandardVersionRequirementsFilters()
			: new Application.Common.Models.StandardVersionRequirementsFilters
			{
				StandardVersionId = filters.StandardVersionId,
                SectionId = filters.SectionId,
				SearchText = filters.SearchText?.Trim(),
                ScopeId = filters.ScopeId,
			};

			return await debService.GetStandardVersionRequirementsForGridAsync(f, cancellationToken);
		}

		[Authorize]
        public static async Task<ICollection<RequirementWithScopes>> GetRequirementScopesForStatement(
            Guid statementId,
            IDebService debService,
            CancellationToken cancellationToken)
            => await debService.GetRequirementScopesForStatement(statementId, cancellationToken);

		[Authorize]
		public static async Task<RequirementDetail?> GetRequirementById(Guid requirementId, IDebService debService, CancellationToken cancellationToken)
			=> await debService.GetRequirementDetailByIdAsync(requirementId, cancellationToken);

		[Authorize]
		public static async Task<RequirementChildCounts> GetChildCountsForRequirement(Guid requirementId, IDebService debService, CancellationToken cancellationToken)
		    => await debService.GetChildCountsForRequirementAsync(requirementId, cancellationToken);
	}
}
