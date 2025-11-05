using HotChocolate.Authorization;
using HotChocolate.Resolvers;
using Nexus.DEB.Application.Common.Interfaces;
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
        
        [NodeResolver]
        public static async Task<TaskType?> GetTaskTypeByIdAsync(
            short id,
            TaskTypeByIdDataLoader taskTypeByIdDataLoader,
            CancellationToken cancellationToken)
            => await taskTypeByIdDataLoader.LoadAsync(id, cancellationToken);
    }
}
