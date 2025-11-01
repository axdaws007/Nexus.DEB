using HotChocolate.Authorization;
using HotChocolate.Resolvers;
using Nexus.DEB.Application.Common.Interfaces;
using Nexus.DEB.Domain.Models;

namespace Nexus.DEB.Api.GraphQL.Scope
{
    [QueryType]
    public static class ScopeQueries
    {
        public static class StandardVersionQueries
        {
            [Authorize]
            [UseOffsetPaging]
            [UseSorting]
            public static IQueryable<ScopeSummary> GetScopesForGrid(
                IDebService debService,
                IResolverContext resolverContext)
                => debService.GetScopesForGrid();

        }
    }
}
