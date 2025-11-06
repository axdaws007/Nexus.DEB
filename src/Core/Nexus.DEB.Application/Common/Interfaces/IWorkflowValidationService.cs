using Nexus.DEB.Application.Common.Models;

namespace Nexus.DEB.Application.Common.Interfaces
{
    public interface IWorkflowValidationService
    {
        Task<Result> ValidateTransitionAsync(
            Guid entityId,
            int stepId,
            int triggerStatusId,
            CancellationToken cancellationToken = default);
    }
}
