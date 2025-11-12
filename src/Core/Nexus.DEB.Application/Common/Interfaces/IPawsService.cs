using Nexus.DEB.Application.Common.Models;

namespace Nexus.DEB.Application.Common.Interfaces
{
    public interface IPawsService
    {
        Task<ICollection<PendingActivity>?> GetPendingActivitiesAsync(
            Guid entityId,
            Guid workflowId,
            CancellationToken cancellationToken = default);

        Task<DestinationActivity?> GetDestinationActivitiesAsync(
            int stepId,
            int triggerStatusId,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyDictionary<Guid, string?>> GetStatusesForEntitiesAsync(
            List<Guid> entityIds,
            CancellationToken cancellationToken = default);

        Task<TransitionConfigurationInfo?> GetTransitionByTriggerAsync(
            int sourceActivityId,
            int triggerStatusId,
            CancellationToken cancellationToken = default);

        Task<ICollection<WorkflowPseudoState>?> GetPseudoStatesByWorkflowAsync(
            Guid workflowId, 
            CancellationToken cancellationToken = default);

        Task<bool> CreateWorkflowInstanceAsync(
            Guid workflowID,
            Guid entityId,
            CancellationToken cancellationToken = default);

        Task<bool> ApproveStepAsync(
            Guid workflowId,
            Guid entityId,
            int stepId,
            int statusId,
            int[] destinationActivityId,
            string? comments = null,
            Guid? onBehalfOfId = null,
            string? password = null,
            Guid[]? defaultOwnerIds = null,
            CancellationToken cancellationToken = default);

        Task<WorkflowHistory>? GetWorkflowHistoryAsync(
            Guid workflowId,
            Guid entityId,
            CancellationToken cancellationToken = default);

        Task<string?> GetWorkflowDiagramHtmlAsync(Guid workflowId, Guid entityId, CancellationToken cancellationToken);

        Task<byte[]?> GetWorkflowDiagramImageAsync(string cacheKey, CancellationToken cancellationToken);
    }
}
