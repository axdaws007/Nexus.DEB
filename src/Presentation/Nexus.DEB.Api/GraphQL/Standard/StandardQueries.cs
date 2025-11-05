using HotChocolate.Authorization;
using HotChocolate.Resolvers;
using Nexus.DEB.Application.Common.Interfaces;
using Nexus.DEB.Domain.Models;

namespace Nexus.DEB.Api.GraphQL
{
    [QueryType]
    public static class StandardQueries
    {
        [Authorize]
        public static IQueryable<Standard> GetStandards(
            IDebService debService,
            IResolverContext resolverContext)
            => debService.GetStandards();

        [NodeResolver]
        public static async Task<Standard?> GetStandardByIdAsync(
            short id,
            StandardByIdDataLoader standardByIdDataLoader,
            CancellationToken cancellationToken)
            => await standardByIdDataLoader.LoadAsync(id, cancellationToken);
    }
}
