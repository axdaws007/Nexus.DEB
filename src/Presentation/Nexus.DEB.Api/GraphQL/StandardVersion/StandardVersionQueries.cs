using HotChocolate.Authorization;
using HotChocolate.Resolvers;
using Nexus.DEB.Application.Common.Interfaces;
using Nexus.DEB.Application.Common.Models;
using Nexus.DEB.Domain.Models;

namespace Nexus.DEB.Api.GraphQL.StandardVersion
{
    [QueryType]
    public static class StandardVersionQueries
    {
        [Authorize]
        [UseOffsetPaging]
        [UseSorting]
        public static IQueryable<StandardVersionSummary> GetStandardVersionsForGrid(
            IDebService debService,
            IResolverContext resolverContext)
            => debService.GetStandardVersionsForGrid();

        [Authorize]
        [UseSorting]
        public static IQueryable<FilterItem> GetStandardVersionsForFilter(
            IDebService debService,
            IResolverContext resolverContext)
            => debService.GetStandardVersionsForFilter();
    }
}
