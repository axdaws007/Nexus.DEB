using HotChocolate.Authorization;
using HotChocolate.Resolvers;
using Nexus.DEB.Application.Common.Interfaces;
using Nexus.DEB.Application.Common.Models.Filters;
using Nexus.DEB.Domain.Models;

namespace Nexus.DEB.Api.GraphQL
{
    [QueryType]
    public static class TaskQueries
    {
        [Authorize]
        [UseOffsetPaging]
        [UseSorting]
        public static IQueryable<TaskSummary> GetTasksForGrid(
            TaskSummaryFilters? filters,
            IDebService debService,
            IResolverContext resolverContext)
            => debService.GetTasksForGrid(filters);
    }
}
