using HotChocolate.Authorization;
using Nexus.DEB.Application.Common.Interfaces;
using Nexus.DEB.Application.Common.Models;
using Nexus.DEB.Application.Common.Models.Events;
using Nexus.DEB.Domain;
using Nexus.DEB.Domain.Interfaces;

namespace Nexus.DEB.Api.GraphQL.Task
{
    [MutationType]
    public static class TaskMutations
    {
        [Authorize(Policy = DebHelper.Policies.CanCreateSoCTask)]
        public static async Task<TaskDetail?> CreateTaskAsync(
            Guid statementId,
            Guid taskOwnerId,
            short taskTypeId,
            int statusId,
            DateOnly? dueDate,
            string title,
            string? description,
            ITaskDomainService taskDomainService,
            IDomainEventPublisher eventPublisher,
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
                EntityType = taskDetail.EntityTypeTitle,
                EntityId = taskDetail.EntityId,
                SerialNumber = taskDetail.SerialNumber ?? string.Empty,
                IsNew = true
            }, cancellationToken);

            return taskDetail;
        }

        [Authorize(Policy = DebHelper.Policies.CanEditSoCTask)]
        public static async Task<TaskDetail?> UpdateTaskAsync(
            Guid id,
            Guid statementId,
            Guid taskOwnerId,
            short taskTypeId,
            int statusId,
            DateOnly? dueDate,
            string title,
            string? description,
            ITaskDomainService taskDomainService,
            IDomainEventPublisher eventPublisher,
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
                EntityType = taskDetail.EntityTypeTitle,
                EntityId = taskDetail.EntityId,
                SerialNumber = taskDetail.SerialNumber ?? string.Empty,
                IsNew = false
            }, cancellationToken);

            return taskDetail;
        }
    }
}
