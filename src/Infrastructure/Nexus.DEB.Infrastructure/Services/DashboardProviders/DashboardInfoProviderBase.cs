using Microsoft.Extensions.Logging;
using Nexus.DEB.Application.Common.Interfaces;
using Nexus.DEB.Application.Common.Models;
using Nexus.DEB.Domain;
using Nexus.DEB.Domain.Models.Common;
using Nexus.DEB.Domain.Models.Other;

namespace Nexus.DEB.Infrastructure.Services
{
    public abstract class DashboardInfoProviderBase : IDashboardInfoProvider
    {
        protected IPawsService PawsService { get; init; }
        protected IDebService DebService { get; init; }
        protected IApplicationSettingsService ApplicationSettingsService { get; init; }
        protected ILogger Logger { get; init; }

        public string EntityType { get; protected set; }

        public DashboardInfoProviderBase(
            IPawsService pawsService,
            IDebService debService,
            IApplicationSettingsService applicationSettingsService,
            ILogger logger,
            string entityType)
        {
            this.PawsService = pawsService;
            this.DebService = debService;
            this.ApplicationSettingsService = applicationSettingsService;
            this.Logger = logger;
            this.EntityType = entityType;
        }

        public virtual async Task<DashboardInfo> CalculateDashboardInfoAsync(object entity, Guid entityId, CancellationToken cancellationToken = default)
        {
            Logger.LogDebug("Calculating dashboard info for EntityId: {EntityId}, EntityType: {EntityType}, EntityProvided: {EntityProvided}",
                entityId, EntityType, entity != null);

            var moduleId = this.ApplicationSettingsService.GetModuleId("DEB");
            var workflowId = await this.DebService.GetWorkflowIdAsync(moduleId, this.EntityType, cancellationToken);

            Logger.LogDebug("Resolved ModuleId: {ModuleId}, WorkflowId: {WorkflowId} for EntityType: {EntityType}",
                moduleId, workflowId, EntityType);

            if (entity == null)
            {
                Logger.LogDebug("Entity not provided for {EntityId}, fetching EntityHead from DebService", entityId);
                entity = await this.DebService.GetEntityHeadAsync(entityId, cancellationToken);

                if (entity == null)
                {
                    Logger.LogWarning("EntityHead lookup returned null for EntityId: {EntityId}", entityId);
                }
            }

            var dashboardInfo = new DashboardInfo()
            {
                EntityId = entityId
            };

            var entityActivitySteps = await this.PawsService.GetEntityActivityStepsAsync(entityId, workflowId.Value, cancellationToken);
            var completedSteps = await this.PawsService.GetCompletedStepsAsync(entityId, workflowId.Value, DebHelper.Paws.MutTags.DashboardOpened, cancellationToken);

            Logger.LogDebug("PAWS returned {ActivityStepCount} activity steps and {CompletedStepCount} completed steps for EntityId: {EntityId}",
                entityActivitySteps?.Count ?? 0, completedSteps?.Count ?? 0, entityId);

            if (entityActivitySteps != null && entityActivitySteps.Count > 0)
            {
                dashboardInfo.IsWorkflowActive = entityActivitySteps.Any(x => x.StatusID == DebHelper.Paws.Status.Pending);

                Logger.LogDebug("Workflow active: {IsWorkflowActive} for EntityId: {EntityId} (Pending steps: {PendingCount})",
                    dashboardInfo.IsWorkflowActive, entityId,
                    entityActivitySteps.Count(x => x.StatusID == DebHelper.Paws.Status.Pending));

                dashboardInfo.AssignedToPostId = await DetermineAssigneeAsync(entity, entityId, entityActivitySteps, cancellationToken);
                dashboardInfo.EntityClosedDate = dashboardInfo.IsWorkflowActive == false ? entityActivitySteps.Max(x => x.UpdatedDate) : null;

                if (dashboardInfo.EntityClosedDate.HasValue)
                {
                    Logger.LogDebug("Workflow closed for EntityId: {EntityId}, ClosedDate: {ClosedDate}",
                        entityId, dashboardInfo.EntityClosedDate);
                }
            }
            else
            {
                Logger.LogDebug("No activity steps found for EntityId: {EntityId}, WorkflowId: {WorkflowId}",
                    entityId, workflowId);
            }

            if (completedSteps != null && completedSteps.Count > 0)
            {
                var openStep = completedSteps.FirstOrDefault();

                dashboardInfo.IsOpen = (dashboardInfo.IsWorkflowActive && openStep != null);
                dashboardInfo.EntityOpenDate = (openStep != null) ? openStep.UpdatedDate : null;

                Logger.LogDebug("Dashboard open state for EntityId: {EntityId} - IsOpen: {IsOpen}, OpenDate: {OpenDate}",
                    entityId, dashboardInfo.IsOpen, dashboardInfo.EntityOpenDate);
            }

            dashboardInfo.DueDate = DetermineDueDate(entity, entityId);
            dashboardInfo.ReviewDate = DetermineReviewDate(entity, entityId);
            dashboardInfo.ResponsibleOwnerId = DetermineResponsibleOwner(entity, entityId);

            Logger.LogDebug("Dashboard info calculated for EntityId: {EntityId} - AssignedTo: {AssignedTo}, ResponsibleOwner: {ResponsibleOwner}, DueDate: {DueDate}, ReviewDate: {ReviewDate}, IsOpen: {IsOpen}, IsWorkflowActive: {IsWorkflowActive}",
                entityId, dashboardInfo.AssignedToPostId, dashboardInfo.ResponsibleOwnerId,
                dashboardInfo.DueDate, dashboardInfo.ReviewDate,
                dashboardInfo.IsOpen, dashboardInfo.IsWorkflowActive);

            return dashboardInfo;
        }

        protected virtual DateOnly? DetermineDueDate(object entity, Guid entityId)
        {
            return null;
        }

        protected virtual DateOnly? DetermineReviewDate(object entity, Guid entityId)
        {
            return null;
        }

        protected virtual Guid? DetermineResponsibleOwner(object entity, Guid entityId)
        {
            Guid? owner = entity switch
            {
                EntityDetailBase entityDetail => entityDetail.OwnedById,
                EntityHead entityHead => entityHead.OwnedById,
                _ => null
            };

            Logger.LogDebug("Determined responsible owner for EntityId: {EntityId} - OwnerId: {OwnerId}, EntityRuntimeType: {EntityType}",
                entityId, owner, entity?.GetType().Name ?? "null");

            return owner;
        }

        protected virtual async Task<Guid?> DetermineAssigneeAsync(object entity, Guid entityId, ICollection<EntityActivityStep> steps, CancellationToken cancellationToken)
        {
            Guid? assignee = null;

            var pendingSteps = steps.Where(x => x.StatusID == DebHelper.Paws.Status.Pending);
            Logger.LogDebug("Determining assignee for EntityId: {EntityId} - {PendingCount} pending steps to check",
                entityId, pendingSteps.Count());

            foreach (var pendingStep in pendingSteps)
            {
                var owner = await this.PawsService.GetEntityActivityOwnerAsync(entityId, pendingStep.ActivityID, cancellationToken);

                Logger.LogDebug("Activity owner lookup for EntityId: {EntityId}, ActivityId: {ActivityId} - OwnerId: {OwnerId}",
                    entityId, pendingStep.ActivityID, owner?.OwnerID);

                if (owner != null)
                {
                    assignee = owner.OwnerID;
                }
            }

            if (!assignee.HasValue)
            {
                Logger.LogDebug("No assignee found from pending steps for EntityId: {EntityId}, falling back to entity owner", entityId);

                switch (entity)
                {
                    case EntityDetailBase detail:
                        assignee = detail.OwnedById;
                        Logger.LogDebug("EntityDetailBase fallback for EntityId: {EntityId} - OwnedById: {OwnedById}", entityId, detail.OwnedById);

                        if (assignee == Guid.Empty)
                        {
                            Logger.LogDebug("OwnedById is empty for EntityId: {EntityId}, fetching EntityHead for CreatedById fallback", entityId);
                            var head = await DebService.GetEntityHeadAsync(entityId, cancellationToken);
                            assignee = head?.CreatedById;
                            Logger.LogDebug("CreatedById fallback for EntityId: {EntityId} - CreatedById: {CreatedById}", entityId, assignee);
                        }
                        break;

                    case EntityHead head:
                        assignee = head.OwnedById != Guid.Empty
                            ? head.OwnedById
                            : head.CreatedById;
                        Logger.LogDebug("EntityHead fallback for EntityId: {EntityId} - resolved to {Assignee} (OwnedById: {OwnedById}, CreatedById: {CreatedById})",
                            entityId, assignee, head.OwnedById, head.CreatedById);
                        break;

                    default:
                        Logger.LogWarning("Unable to determine assignee for EntityId: {EntityId} - entity type {EntityType} has no fallback logic",
                            entityId, entity?.GetType().Name ?? "null");
                        break;
                }
            }

            Logger.LogDebug("Final assignee for EntityId: {EntityId}: {Assignee}", entityId, assignee);
            return assignee;
        }
    }
}