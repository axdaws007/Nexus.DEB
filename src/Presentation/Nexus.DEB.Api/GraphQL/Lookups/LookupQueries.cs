using HotChocolate.Authorization;
using HotChocolate.Resolvers;
using Nexus.DEB.Application.Common.Interfaces;
using Nexus.DEB.Domain.Models;

namespace Nexus.DEB.Api.GraphQL
{
    [QueryType]
    public static class LookupQueries
    {
        [Authorize]
        public static IQueryable<Standard> GetStandards(
            IDebService debService,
            IResolverContext resolverContext)
            => debService.GetStandards();

        [Authorize]
        public static IQueryable<TaskType> GetTaskTypes(
            IDebService debService,
            IResolverContext resolverContext)
            => debService.GetTaskTypes();
    }
}
