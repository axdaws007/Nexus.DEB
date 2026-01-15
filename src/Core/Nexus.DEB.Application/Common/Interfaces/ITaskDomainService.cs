using Nexus.DEB.Application.Common.Models;

namespace Nexus.DEB.Application.Common.Interfaces
{
    public interface ITaskDomainService
    {
        Task<Result<TaskDetail>> CreateTaskAsync(
            Guid statementId,
            Guid taskOwnerId,
            short taskTypeId,
            int activityId,
            DateOnly? dueDate,
            string title,
            string? description,
            CancellationToken cancellationToken = default);

        Task<Result<TaskDetail>> UpdateTaskAsync(
            Guid id,
            Guid statementId,
            Guid taskOwnerId,
            short taskTypeId,
            int activityId,
            DateOnly? dueDate,
            string title,
            string? description,
            CancellationToken cancellationToken = default);
    }
}
