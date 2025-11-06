using Nexus.DEB.Application.Common.Models;

namespace Nexus.DEB.Application.Common.Interfaces
{
    public interface IWorkflowSideEffectService
    {
        Task<Result> ExecuteSideEffectAsync(
            Guid entityId,
            int stepId,
            int triggerStatusId,
            CancellationToken cancellationToken = default);
    }
}
