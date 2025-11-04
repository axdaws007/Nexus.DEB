using HotChocolate.Authorization;
using HotChocolate.Resolvers;
using Nexus.DEB.Application.Common.Interfaces;
using Nexus.DEB.Application.Common.Models;

namespace Nexus.DEB.Api.GraphQL.Standard
{
    [QueryType]
    public static class StandardQueries
    {
        [Authorize]
        [UseSorting]
        public static IQueryable<FilterItem<short>> GetStandardsForFilter(
            IDebService debService,
            IResolverContext resolverContext)
            => debService.GetStandardsForFilter();
    }
}
