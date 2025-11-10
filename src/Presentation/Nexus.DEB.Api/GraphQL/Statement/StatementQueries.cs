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
            IResolverContext resolverContext)
            => debService.GetStatementsForGrid(filters);

        [Authorize]
        public static async Task<StatementDetail?> GetStatementById(
            Guid id,
            IDebService debService,
            IResolverContext resolverContext,
            CancellationToken cancellationToken)
            => await debService.GetStatementByIdAsync(id, cancellationToken);
    }
}
