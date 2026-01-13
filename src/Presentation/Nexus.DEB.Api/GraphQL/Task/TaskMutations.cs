using HotChocolate.Authorization;
using Nexus.DEB.Application.Common.Interfaces;
using Nexus.DEB.Application.Common.Models;
using Nexus.DEB.Domain;
using Nexus.DEB.Domain.Interfaces;
using Nexus.DEB.Domain.Models.Common;
using Nexus.DEB.Domain.Models.Events;

namespace Nexus.DEB.Api.GraphQL.Task
{
    [MutationType]
    public static class TaskMutations
    {
        [Authorize(Policy = DebHelper.Policies.CanCreateSoCTask)]
        public static async Task<TaskDetail?> CreateTaskAsync(
            Guid statementId,
            Guid taskOwnerId,
            [ID]short taskTypeId,
            [ID]int statusId,
            DateTime? dueDate,
            string title,
            string? description,
            ITaskDomainService taskDomainService,
            IDomainEventPublisher eventPublisher,
            ICurrentUserService currentUserService,
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

            var taskDetail = result.Data!;

            await eventPublisher.PublishAsync(new EntitySavedEvent
            {
                Entity = taskDetail,
                EntityType = EntityTypes.Task,
                EntityId = taskDetail.EntityId,
                SerialNumber = taskDetail.SerialNumber ?? string.Empty,
                IsNew = true,
                UserId = currentUserService.UserId
            }, cancellationToken);

            return taskDetail;
        }

        [Authorize(Policy = DebHelper.Policies.CanEditSoCTask)]
        public static async Task<TaskDetail?> UpdateTaskAsync(
            Guid id,
            Guid statementId,
            Guid taskOwnerId,
            [ID] short taskTypeId,
            [ID] int statusId,
            DateTime? dueDate,
            string title,
            string? description,
            ITaskDomainService taskDomainService,
            IDomainEventPublisher eventPublisher,
            ICurrentUserService currentUserService,
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

            var taskDetail = result.Data!;

            await eventPublisher.PublishAsync(new EntitySavedEvent
            {
                Entity = taskDetail,
                EntityType = EntityTypes.Task,
                EntityId = taskDetail.EntityId,
                SerialNumber = taskDetail.SerialNumber ?? string.Empty,
                IsNew = false,
                UserId = currentUserService.UserId
            }, cancellationToken);

            return taskDetail;
        }
    }
}
