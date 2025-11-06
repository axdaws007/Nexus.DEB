using HotChocolate.Authorization;
using HotChocolate.Resolvers;
using Nexus.DEB.Application.Common.Interfaces;
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
            ICisService cisService,
            IResolverContext resolverContext)
        {
            var f = new Application.Common.Models.Filters.TaskSummaryFilters()
            {
                DueDateFrom = filters.DueDateFrom,
                DueDateTo = filters.DueDateTo,
                SearchText = filters.SearchText,
                StandardVersionIds = filters.StandardVersionIds,
                StatementId = filters.StatementId,
                StatusIds = filters.StatusIds,
                TaskTypeIds = filters.TaskTypeIds
            };

            if (!string.IsNullOrEmpty(filters.OwnedBy))
            {
                var posts = cisService.GetPostsBySearchTextAsync(filters.OwnedBy).GetAwaiter().GetResult();

                if (posts != null && posts.Count > 0)
                {
                    f.OwnedByIds = [.. posts.Select(x => x.ID)];
                }
            }

            return debService.GetTasksForExportOrGrid(f);
        }
    }
}
