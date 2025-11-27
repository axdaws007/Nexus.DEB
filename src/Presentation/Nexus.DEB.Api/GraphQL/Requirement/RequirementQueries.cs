using HotChocolate.Authorization;
using Nexus.DEB.Application.Common.Interfaces;
using Nexus.DEB.Application.Common.Models;
using Nexus.DEB.Domain.Models;
using Nexus.DEB.Infrastructure.Services;
using System.Threading;

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
                    StatementId = filters.StatementId,
                    StatusIds = filters.StatusIds
                };

            return debService.GetRequirementsForGrid(f);
        }

        //[Authorize]
        //public static async Task<ICollection<RequirementWithScopes>> GetRequirementScopesForStatement(
        //    Guid statementId,
        //    IDebService debService,
        //    CancellationToken cancellationToken)
        //    => await debService.GetRequirementScopesForStatement(statementId, cancellationToken);
    }
}
