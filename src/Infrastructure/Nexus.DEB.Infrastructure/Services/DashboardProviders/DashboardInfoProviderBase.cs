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

        public string EntityType { get; protected set; }

        public DashboardInfoProviderBase(IPawsService pawsService, IDebService debService, IApplicationSettingsService applicationSettingsService, string entityType)
        {
            this.PawsService = pawsService;
            this.DebService = debService;
            this.ApplicationSettingsService = applicationSettingsService;
            this.EntityType = entityType;
        }

        public virtual async Task<DashboardInfo> CalculateDashboardInfoAsync(object entity, Guid entityId, CancellationToken cancellationToken = default)
        {
            var moduleId = this.ApplicationSettingsService.GetModuleId("DEB");
            var workflowId = await this.DebService.GetWorkflowIdAsync(moduleId, this.EntityType, cancellationToken);

            if (entity == null)
            {
                entity = await this.DebService.GetEntityHeadAsync(entityId, cancellationToken);
            }

            var dashboardInfo = new DashboardInfo()
            {
                EntityId = entityId
            };

            var entityActivitySteps = await this.PawsService.GetEntityActivityStepsAsync(entityId, workflowId.Value, cancellationToken);
            var completedSteps = await this.PawsService.GetCompletedStepsAsync(entityId, workflowId.Value, DebHelper.Paws.MutTags.DashboardOpened, cancellationToken);

            if (entityActivitySteps != null && entityActivitySteps.Count > 0)
            {
                dashboardInfo.IsWorkflowActive = entityActivitySteps.Any(x => x.StatusID == DebHelper.Paws.Status.Pending);
                dashboardInfo.AssignedToPostId = await DetermineAssigneeAsync(entity, entityId, entityActivitySteps, cancellationToken);
                dashboardInfo.EntityClosedDate = dashboardInfo.IsWorkflowActive == false ? entityActivitySteps.Max(x => x.UpdatedDate) : null;
            }

            if (completedSteps != null && completedSteps.Count > 0)
            {
                var openStep = completedSteps.FirstOrDefault();

                dashboardInfo.IsOpen = (dashboardInfo.IsWorkflowActive && openStep != null);
                dashboardInfo.EntityOpenDate = (openStep != null) ? openStep.UpdatedDate : null;
            }

            dashboardInfo.DueDate = DetermineDueDate(entity, entityId);
            dashboardInfo.ReviewDate = DetermineReviewDate(entity, entityId);
            dashboardInfo.ResponsibleOwnerId = DetermineResponsibleOwner(entity, entityId);

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
            return entity switch
            {
                EntityDetailBase entityDetail => entityDetail.OwnedById,
                EntityHead entityHead => entityHead.OwnedById,
                _ => null
            };
        }

        protected virtual async Task<Guid?> DetermineAssigneeAsync(object entity, Guid entityId, ICollection<EntityActivityStep> steps, CancellationToken cancellationToken)
        {
            Guid? assignee = null;

            var pendingSteps = steps.Where(x => x.StatusID == DebHelper.Paws.Status.Pending);

            foreach(var pendingStep in pendingSteps)
            {
                var owner = await this.PawsService.GetEntityActivityOwnerAsync(entityId, pendingStep.ActivityID, cancellationToken);

                if (owner != null)
                {
                    assignee = owner.OwnerID;
                }
            }

            if (!assignee.HasValue)
            {
                switch (entity)
                {
                    case EntityDetailBase detail:
                        assignee = detail.OwnedById;

                        if (assignee == Guid.Empty)
                        {
                            var head = await DebService.GetEntityHeadAsync(entityId, cancellationToken);
                            assignee = head?.CreatedById;
                        }
                        break;

                    case EntityHead head:
                        assignee = head.OwnedById != Guid.Empty
                            ? head.OwnedById
                            : head.CreatedById;
                        break;
                }
            }

            return assignee;
        }
    }
}
