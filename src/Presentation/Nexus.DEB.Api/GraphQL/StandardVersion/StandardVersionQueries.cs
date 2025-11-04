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
            => debService.GetStandardVersionsForGrid(filters);

        [Authorize]
        [UseSorting]
        public static IQueryable<StandardVersion> GetStandardVersions(
            IDebService debService,
            IResolverContext resolverContext)
            => debService.GetStandardVersions();
    }
}
