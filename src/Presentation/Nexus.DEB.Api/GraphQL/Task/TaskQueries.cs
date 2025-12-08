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
            var f = filters is null
            ? new Application.Common.Models.Filters.TaskSummaryFilters()
            : new Application.Common.Models.Filters.TaskSummaryFilters
            {
                DueDateFrom = filters.DueDateFrom,
                DueDateTo = filters.DueDateTo,
                SearchText = filters.SearchText?.Trim(),
                StandardVersionIds = filters.StandardVersionIds,
                StatementId = filters.StatementId,
                StatusIds = filters.StatusIds,
                TaskTypeIds = filters.TaskTypeIds
            };

            if (!string.IsNullOrEmpty(filters?.OwnedBy))
            {
                var posts = cisService.GetPostsBySearchTextAsync(filters.OwnedBy).GetAwaiter().GetResult();

                if (posts != null && posts.Count > 0)
                {
                    f.OwnedByIds = [.. posts.Select(x => x.ID)];
                }
            }

            return debService.GetTasksForGrid(f);
        }

        [Authorize]
        public static async Task<TaskDetailView?> GetTaskById(Guid taskId,
			IDebService debService,
			CancellationToken cancellationToken)
			=> await debService.GetTaskDetailByIdAsync(taskId, cancellationToken);
	}
}
