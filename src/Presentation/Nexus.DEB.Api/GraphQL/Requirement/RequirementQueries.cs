using HotChocolate.Authorization;
using HotChocolate.Resolvers;
using Nexus.DEB.Application.Common.Interfaces;
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
        {
            var f = filters is null
                ? new Application.Common.Models.Filters.RequirementSummaryFilters()
                : new Application.Common.Models.Filters.RequirementSummaryFilters
                {
                    ModifiedFrom = filters.ModifiedFrom,
                    ModifiedTo = filters.ModifiedTo,
                    ScopeIds = filters.ScopeIds,
                    SearchText = filters.SearchText?.Trim(),
                    StandardVersionIds = filters.StandardVersionIds,
                    StatusIds = filters.StatusIds
                };

            return debService.GetRequirementsForGrid(f);
        }
    }
}
