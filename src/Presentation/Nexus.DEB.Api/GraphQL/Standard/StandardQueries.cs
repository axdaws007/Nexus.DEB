using HotChocolate.Authorization;
using HotChocolate.Resolvers;
using Nexus.DEB.Application.Common.Interfaces;
using Nexus.DEB.Application.Common.Models.Filters;
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
            short standardId,
            StandardByIdDataLoader standardByIdDataLoader,
            CancellationToken cancellationToken)
            => await standardByIdDataLoader.LoadAsync(standardId, cancellationToken);

        [Authorize]
        public async static Task<ICollection<FilterItem>> GetStandardsLookupAsync(
            IDebService debService,
            IResolverContext resolverContext,
            CancellationToken cancellationToken)
            => await debService.GetStandardsLookupAsync(cancellationToken);
    }
}
