using HotChocolate.Authorization;
using HotChocolate.Resolvers;
using Nexus.DEB.Application.Common.Interfaces;
using Nexus.DEB.Application.Common.Models;
using Nexus.DEB.Domain.Models;

namespace Nexus.DEB.Api.GraphQL.Scope
{
    [QueryType]
    public static class ScopeQueries
    {
        [Authorize]
        [UseOffsetPaging]
        [UseSorting]
        public static IQueryable<ScopeSummary> GetScopesForGrid(
            IDebService debService,
            IResolverContext resolverContext)
            => debService.GetScopesForGrid();

        [Authorize]
        [UseSorting]
        public static IQueryable<FilterItem> GetScopesForFilter(
            IDebService debService,
            IResolverContext resolverContext)
            => debService.GetScopesForFilter();
    }
}
