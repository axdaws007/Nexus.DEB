using HotChocolate.Authorization;
using HotChocolate.Resolvers;
using Nexus.DEB.Application.Common.Interfaces;
using Nexus.DEB.Application.Common.Models.Filters;
using Nexus.DEB.Domain.Models;

namespace Nexus.DEB.Api.GraphQL
{
    [QueryType]
    public static class StandardVersionQueries
    {
        [Authorize]
        [UseOffsetPaging]
        [UseSorting]
        public static IQueryable<StandardVersionSummary> GetStandardVersionsForGrid(
            StandardVersionSummaryFilters? filters,
            IDebService debService,
            IResolverContext resolverContext)
        {
            var f = filters is null
            ? new Application.Common.Models.Filters.StandardVersionSummaryFilters()
            : new Application.Common.Models.Filters.StandardVersionSummaryFilters
            {
                StandardIds = filters.StandardIds,
                EffectiveFromDate = filters.EffectiveFromDate,  
                EffectiveToDate = filters.EffectiveToDate,  
                StatusIds = filters.StatusIds
            };

            return debService.GetStandardVersionsForGrid(f);
        }

        [Authorize]
        [UseSorting]
        public static IQueryable<StandardVersionSummary> GetStandardVersionsForExport(
            StandardVersionSummaryFilters? filters,
            IDebService debService,
            IResolverContext resolverContext)
        {
            Application.Common.Models.Filters.StandardVersionSummaryFilters f = new Application.Common.Models.Filters.StandardVersionSummaryFilters()
            {
                StandardIds = filters.StandardIds,
                EffectiveFromDate = filters.EffectiveFromDate,
                EffectiveToDate = filters.EffectiveToDate,
                StatusIds = filters.StatusIds
            };

            return debService.GetStandardVersionsForGrid(f);
        }

        [Authorize]
        [UseSorting]
        public static IQueryable<StandardVersion> GetStandardVersions(
            IDebService debService,
            IResolverContext resolverContext)
            => debService.GetStandardVersions();

        [Authorize]
        public async static Task<ICollection<FilterItemEntity>> GetStandardVersionsLookupAsync(
            IDebService debService,
            IResolverContext resolverContext,
            CancellationToken cancellationToken)
            => await debService.GetStandardVersionsLookupAsync(cancellationToken);

    }
}
