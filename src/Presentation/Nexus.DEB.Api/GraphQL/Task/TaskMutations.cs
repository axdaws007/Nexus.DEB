using HotChocolate.Authorization;
using Nexus.DEB.Application.Common.Interfaces;
using Nexus.DEB.Domain;

namespace Nexus.DEB.Api.GraphQL.Task
{
    [MutationType]
    public static class TaskMutations
    {
        [Authorize(Policy = DebHelper.Policies.CanCreateSoCTask)]
        public static async Task<Domain.Models.Task?> CreateTaskAsync(
            Guid statementId,
            Guid taskOwnerId,
            [ID]short taskTypeId,
            [ID]int statusId,
            DateTime? dueDate,
            string title,
            string? description,
            ITaskDomainService taskDomainService,
            CancellationToken cancellationToken = default)
        {
            var result = await taskDomainService.CreateTaskAsync(
                statementId,
                taskOwnerId,
                taskTypeId,
                statusId,
                dueDate,
                title,
                description,
                cancellationToken);

            if (!result.IsSuccess)
            {
                throw ExceptionHelper.BuildException(result);
            }

            return result.Data;
        }

        [Authorize(Policy = DebHelper.Policies.CanEditSoCTask)]
        public static async Task<Domain.Models.Task?> UpdateTaskAsync(
            Guid id,
            Guid statementId,
            Guid taskOwnerId,
            [ID] short taskTypeId,
            [ID] int statusId,
            DateTime? dueDate,
            string title,
            string? description,
            ITaskDomainService taskDomainService,
            CancellationToken cancellationToken = default)
        {
            var result = await taskDomainService.UpdateTaskAsync(
                id,
                statementId,
                taskOwnerId,
                taskTypeId,
                statusId,
                dueDate,
                title,
                description,
                cancellationToken);

            if (!result.IsSuccess)
            {
                throw ExceptionHelper.BuildException(result);
            }

            return result.Data;
        }
    }
}
