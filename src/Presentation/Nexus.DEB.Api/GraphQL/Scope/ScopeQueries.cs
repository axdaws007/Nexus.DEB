using HotChocolate.Authorization;
using HotChocolate.Resolvers;
using Nexus.DEB.Application.Common.Interfaces;
using Nexus.DEB.Application.Common.Models.Filters;
using Nexus.DEB.Domain.Models;

namespace Nexus.DEB.Api.GraphQL
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
        public static IQueryable<Scope> GetScopes(
            IDebService debService,
            IResolverContext resolverContext)
            => debService.GetScopes();

        [Authorize]
        public async static Task<ICollection<FilterItemEntity>> GetScopesLookupAsync(
            IDebService debService,
            IResolverContext resolverContext,
            CancellationToken cancellationToken)
            => await debService.GetScopesLookupAsync(cancellationToken);
    }
}
