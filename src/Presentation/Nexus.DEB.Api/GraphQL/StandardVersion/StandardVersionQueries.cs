using HotChocolate.Authorization;
using HotChocolate.Resolvers;
using Nexus.DEB.Application.Common.Interfaces;
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

        public static string GetStatus(
            IPawsService pawsService,
            IResolverContext resolverContext)
            => pawsService.GetStatusForEntity(Guid.NewGuid());
    }
}
