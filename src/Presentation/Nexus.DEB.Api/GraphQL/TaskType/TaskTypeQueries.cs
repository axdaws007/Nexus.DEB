using HotChocolate.Authorization;
using HotChocolate.Resolvers;
using Nexus.DEB.Application.Common.Interfaces;
using Nexus.DEB.Application.Common.Models.Filters;
using Nexus.DEB.Domain.Models;

namespace Nexus.DEB.Api.GraphQL
{
    [QueryType]
    public static class TaskTypeQueries
    {
        [Authorize]
        public static IQueryable<TaskType> GetTaskTypes(
            IDebService debService,
            IResolverContext resolverContext)
            => debService.GetTaskTypes(); 
        
        [Authorize]
        public async static Task<ICollection<FilterItem>> GetTaskTypesLookupAsync(
            IDebService debService,
            IResolverContext resolverContext,
            CancellationToken cancellationToken)
            => await debService.GetTaskTypesLookupAsync(cancellationToken);


    }
}
