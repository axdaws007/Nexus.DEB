using HotChocolate.Authorization;
using Nexus.DEB.Application.Common.Interfaces;
using Nexus.DEB.Application.Common.Models;
using Nexus.DEB.Application.Common.Models.Events;
using Nexus.DEB.Domain.Interfaces;

namespace Nexus.DEB.Api.GraphQL.Paws
{
    [MutationType]
    public static class WorkflowMutations
    {
        [Authorize]
        public static async Task<CurrentWorkflowStatus> ApproveStepAsync(
            Guid entityId,
            int stepId,
            int triggerStatusId,
            ICollection<ActivityApproval> activitiesToApprove,
            string? comments,
            Guid? onBehalfOfId,
            string? password,
            IDebService debService,
            IPawsService pawsService,
            IWorkflowSideEffectService workflowSideEffectService,
            IApplicationSettingsService applicationSettingsService,
            IDomainEventPublisher eventPublisher,
            ILoggerFactory loggerFactory,
            CancellationToken cancellationToken)
        {
            // Have to create the logger this way because the class is static and you can't use static types as type arguments so ILogger can't be injected in the "usual" way.
            var logger = loggerFactory.CreateLogger("WorkflowMutations");
            Result result;

            logger.LogDebug("ApproveStep started - EntityId: {EntityId}, StepId: {StepId}, TriggerStatusId: {TriggerStatusId}, ActivitiesToApprove: {ActivityCount}, OnBehalfOfId: {OnBehalfOfId}, HasComments: {HasComments}",
                entityId, stepId, triggerStatusId, activitiesToApprove.Count, onBehalfOfId, comments != null);

            var moduleId = applicationSettingsService.GetModuleId("DEB");

            var entity = await debService.GetEntityHeadAsync(entityId, cancellationToken);

            if (entity == null)
            {
                logger.LogWarning("ApproveStep failed - Entity not found for EntityId: {EntityId}", entityId);

                result = Result.Failure(new ValidationError
                {
                    Field = "entity",
                    Message = "Entity not found",
                    Code = "ENTITY_NOT_FOUND"
                });

                throw BuildException(result);
            }

            logger.LogDebug("Entity resolved - EntityId: {EntityId}, EntityType: {EntityType}, SerialNumber: {SerialNumber}",
                entityId, entity.EntityTypeTitle, entity.SerialNumber);

            var workflowId = await debService.GetWorkflowIdAsync(moduleId, entity.EntityTypeTitle, cancellationToken);

            if (workflowId == null)
            {
                logger.LogWarning("ApproveStep failed - Workflow not found for ModuleId: {ModuleId}, EntityType: {EntityType}",
                    moduleId, entity.EntityTypeTitle);

                result = Result.Failure(new ValidationError
                {
                    Field = "workflowId",
                    Message = "Workflow not found",
                    Code = "WORKFLOW_NOT_FOUND"
                });

                throw BuildException(result);
            }

            logger.LogDebug("Workflow resolved - WorkflowId: {WorkflowId} for EntityType: {EntityType}", workflowId, entity.EntityTypeTitle);

            var destinationActivityIds = activitiesToApprove.Select(x => x.ActivityId).ToArray();
            var defaultOwnerIds = activitiesToApprove.Where(x => x.DefaultOwnerId.HasValue).Select(x => x.DefaultOwnerId.Value).ToArray();

            logger.LogDebug("Approving step via PAWS - EntityId: {EntityId}, WorkflowId: {WorkflowId}, StepId: {StepId}, TriggerStatusId: {TriggerStatusId}, DestinationActivityIds: [{DestinationActivityIds}], DefaultOwnerIds: [{DefaultOwnerIds}]",
                entityId, workflowId, stepId, triggerStatusId,
                string.Join(", ", destinationActivityIds),
                string.Join(", ", defaultOwnerIds));

            var approved = await pawsService.ApproveStepAsync(
                workflowId.Value,
                entityId,
                stepId,
                triggerStatusId,
                destinationActivityIds,
                comments,
                onBehalfOfId,
                password,
                defaultOwnerIds,
                cancellationToken);

            if (!approved)
            {
                logger.LogWarning("ApproveStep failed - PAWS rejected approval for EntityId: {EntityId}, StepId: {StepId}, TriggerStatusId: {TriggerStatusId}",
                    entityId, stepId, triggerStatusId);

                result = Result.Failure(new ValidationError
                {
                    Field = "statusId",
                    Message = "Workflow step could not be approved",
                    Code = "WORKFLOW_NOT_APPROVED"
                });

                throw BuildException(result);
            }

            logger.LogDebug("PAWS approval succeeded for EntityId: {EntityId}, StepId: {StepId}. Executing side effects...",
                entityId, stepId);

            result = await workflowSideEffectService.ExecuteSideEffectAsync(entityId, stepId, triggerStatusId, cancellationToken);

            if (!result.IsSuccess)
            {
                logger.LogWarning("ApproveStep failed - Side effect execution failed for EntityId: {EntityId}, StepId: {StepId}, TriggerStatusId: {TriggerStatusId}, Errors: {Errors}",
                    entityId, stepId, triggerStatusId,
                    string.Join("; ", result.Errors.Select(e => $"{e.Code}: {e.Message}")));

                throw BuildException(result);
            }

            logger.LogDebug("Side effects executed successfully for EntityId: {EntityId}, StepId: {StepId}", entityId, stepId);

            var pawsEntityDetails = await debService.GetCurrentWorkflowStatusForEntityAsync(entityId, cancellationToken);

            var currentWorkflowStatus = new CurrentWorkflowStatus();

            currentWorkflowStatus.ActivityId = pawsEntityDetails.ActivityId;
            currentWorkflowStatus.ActivityTitle = pawsEntityDetails.ActivityTitle;
            currentWorkflowStatus.PseudoStateId = pawsEntityDetails.PseudoStateId;
            currentWorkflowStatus.PseudoStateTitle = pawsEntityDetails.PseudoStateTitle;
            currentWorkflowStatus.StatusId = pawsEntityDetails.StatusId;
            currentWorkflowStatus.StatusTitle = pawsEntityDetails.StatusTitle;
            currentWorkflowStatus.StepId = pawsEntityDetails.StepId;

            logger.LogDebug("Post-approval workflow status for EntityId: {EntityId} - ActivityId: {ActivityId}, ActivityTitle: {ActivityTitle}, StatusId: {StatusId}, StatusTitle: {StatusTitle}, PseudoStateId: {PseudoStateId}, StepId: {StepId}",
                entityId, currentWorkflowStatus.ActivityId, currentWorkflowStatus.ActivityTitle,
                currentWorkflowStatus.StatusId, currentWorkflowStatus.StatusTitle,
                currentWorkflowStatus.PseudoStateId, currentWorkflowStatus.StepId);

            if (currentWorkflowStatus.StatusId == 1)
            {
                logger.LogDebug("Status is Pending (1) for EntityId: {EntityId}, fetching pending activities for available triggers", entityId);

                var pendingActivities = await pawsService.GetPendingActivitiesAsync(entityId, workflowId.Value, cancellationToken);

                var selectedPendingActivity = pendingActivities.FirstOrDefault(x => x.ActivityID == currentWorkflowStatus.ActivityId);

                if (selectedPendingActivity == null)
                {
                    logger.LogWarning("No matching pending activity found for EntityId: {EntityId}, ActivityId: {ActivityId} among {PendingCount} pending activities",
                        entityId, currentWorkflowStatus.ActivityId, pendingActivities.Count);
                }
                else
                {
                    logger.LogDebug("Found pending activity for EntityId: {EntityId}, ActivityId: {ActivityId} with {TriggerCount} available trigger states",
                        entityId, currentWorkflowStatus.ActivityId, selectedPendingActivity.AvailableTriggerStates?.Count ?? 0);
                }

                currentWorkflowStatus.AvailableTriggerStates = selectedPendingActivity.AvailableTriggerStates;
            }

            logger.LogDebug("Publishing WorkflowTransitionCompletedEvent for EntityId: {EntityId}, WorkflowId: {WorkflowId}", entityId, workflowId);

            await eventPublisher.PublishAsync(new WorkflowTransitionCompletedEvent
            {
                Entity = entity,
                EntityType = entity.EntityTypeTitle,
                EntityId = entity.EntityId,
                SerialNumber = entity.SerialNumber ?? string.Empty,
                OccurredAt = DateTime.UtcNow,
                CurrentWorkflowStatus = currentWorkflowStatus,
                WorkflowId = workflowId.Value
            }, cancellationToken);

            logger.LogDebug("ApproveStep completed successfully for EntityId: {EntityId}, StepId: {StepId} -> New ActivityId: {ActivityId}, StatusId: {StatusId}",
                entityId, stepId, currentWorkflowStatus.ActivityId, currentWorkflowStatus.StatusId);

            return currentWorkflowStatus;
        }

        private static GraphQLException BuildException(Result result)
        {
            var errors = result.Errors.Select(e =>
                ErrorBuilder.New()
                    .SetMessage(e.Message)
                    .SetCode(e.Code)
                    .SetExtension("field", e.Field)
                    .SetExtension("meta", e.Meta)
                    .Build());

            return new GraphQLException(errors);
        }
    }
}