using Nexus.DEB.Application.Common.Models;

namespace Nexus.DEB.Application.Common.Interfaces
{
    public interface ITaskDomainService
    {
        Task<Result<Domain.Models.Task>> CreateTaskAsync(
            Guid statementId,
            Guid taskOwnerId,
            short taskTypeId,
            int activityId,
            DateTime? dueDate,
            string title,
            string? description,
            CancellationToken cancellationToken = default);

        Task<Result<Domain.Models.Task>> UpdateTaskAsync(
            Guid id,
            Guid statementId,
            Guid taskOwnerId,
            short taskTypeId,
            int activityId,
            DateTime? dueDate,
            string title,
            string? description,
            CancellationToken cancellationToken = default);
    }
}
