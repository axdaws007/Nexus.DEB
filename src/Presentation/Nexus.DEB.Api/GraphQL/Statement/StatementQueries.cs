using HotChocolate.Authorization;
using HotChocolate.Resolvers;
using Nexus.DEB.Application.Common.Interfaces;
using Nexus.DEB.Application.Common.Models;
using Nexus.DEB.Domain.Models;

namespace Nexus.DEB.Api.GraphQL
{
    [QueryType]
    public static class StatementQueries
    {
        [Authorize]
        [UseOffsetPaging]
        [UseSorting]
        public static IQueryable<StatementSummary> GetStatementsForGrid(
            StatementSummaryFilters? filters,
            IDebService debService,
            ICisService cisService,
            IResolverContext resolverContext)
        {
            var f = filters is null
            ? new Application.Common.Models.StatementSummaryFilters()
            : new Application.Common.Models.StatementSummaryFilters
            {
                ModifiedFrom = filters.ModifiedFrom,
                ModifiedTo = filters.ModifiedTo,
                ScopeIds = filters.ScopeIds,
                SearchText = filters.SearchText?.Trim(),
                StandardVersionIds = filters.StandardVersionIds,
                StatusIds = filters.StatusIds,
            };

            if (!string.IsNullOrEmpty(filters?.OwnedBy))
            {
                var posts = cisService.GetPostsBySearchTextAsync(filters.OwnedBy).GetAwaiter().GetResult();

                if (posts != null && posts.Count > 0)
                {
                    f.OwnedByIds = [.. posts.Select(x => x.ID)];
                }
            }

            return debService.GetStatementsForGrid(f);
        }

        [Authorize]
        public static async Task<StatementDetail?> GetStatementById(
            Guid id,
            IDebService debService,
            IResolverContext resolverContext,
            CancellationToken cancellationToken)
            => await debService.GetStatementDetailByIdAsync(id, cancellationToken);
    }
}
