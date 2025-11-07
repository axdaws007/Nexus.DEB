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

        Task<bool> CreateWorkflowInstance(
            Guid workflowID,
            Guid entityIds);

        //// Existing execute transition method...
        //Task<Result<WorkflowTransitionResult>> ExecuteTransitionAsync(
        //    WorkflowTransitionRequest request,
        //    CancellationToken cancellationToken = default);
    }
}
