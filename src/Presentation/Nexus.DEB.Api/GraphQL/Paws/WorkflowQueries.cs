using HotChocolate.Authorization;
using HotChocolate.Resolvers;
using Nexus.DEB.Application.Common.Interfaces;
using Nexus.DEB.Application.Common.Models;
using Nexus.DEB.Application.Common.Models.Filters;
using Nexus.DEB.Domain.Models;
using Nexus.DEB.Domain.Models.Common;
using Nexus.DEB.Infrastructure.Helpers;

namespace Nexus.DEB.Api.GraphQL.Paws
{
    [QueryType]
    public static class WorkflowQueries
    {
        [Authorize]
        public static async Task<TransitionDetail> ValidateWorkflowTransition(
            Guid entityId,
            int stepId,
            int triggerStatusId,
            IPawsService pawsService,
            IWorkflowValidationService validationService,
            CancellationToken cancellationToken)
        {
            var destinationActivity = await pawsService.GetDestinationActivitiesAsync(stepId, triggerStatusId, cancellationToken);

            var result = await validationService.ValidateTransitionAsync(
                entityId,
                triggerStatusId,
                destinationActivity.TargetActivities,
                cancellationToken);

            return new TransitionDetail
            {
                RequirePassword = destinationActivity.RequirePassword,
                ShowSignOffText = destinationActivity.ShowSignoffText,
                SignOffText = destinationActivity.SignoffText,
                TargetActivities = destinationActivity.TargetActivities,
                ValidationSuccessful = result.IsSuccess,
                ValidationErrors = result.Errors.Select(e => new ValidationError
                {
                    Message = e.Message,
                    Code = e.Code,
                    Field = e.Field
                }).ToList()
            };
        }

        [Authorize]
        public static async Task<PawsState?> GetWorkflowStatusByIdAsync(
            Guid id,
            IDebService debService,
            CancellationToken cancellationToken)
            => await debService.GetWorkflowStatusByIdAsync(id, cancellationToken);

        [Authorize]
        public static async Task<CurrentWorkflowStatus?> GetCurrentWorkflowStatusForEntityAsync(
            Guid entityId,
            IDebService debService,
            IPawsService pawsService,
            ICbacService cbacService,
            IResolverContext resolverContext,
            IApplicationSettingsService applicationSettingsService,
            CancellationToken cancellationToken)
        {
            var debUser = new DebUser(resolverContext.GetUser());
            var moduleId = applicationSettingsService.GetModuleId("DEB");
            var entity = await debService.GetEntityHeadAsync(entityId, cancellationToken);

            if (entity == null)
            {
                throw new InvalidOperationException("EntityID could not be identified");
            }

            var workflowId = await debService.GetWorkflowIdAsync(moduleId, entity.EntityTypeTitle, cancellationToken);

            if (workflowId.HasValue == false)
            {
                throw new InvalidOperationException("WorkflowID could not be identified");
            }

            var pawsEntityDetails = await debService.GetCurrentWorkflowStatusForEntityAsync(entityId, cancellationToken);

            var currentWorkflowStatus = new CurrentWorkflowStatus();

            currentWorkflowStatus.ActivityId = pawsEntityDetails.ActivityId;
            currentWorkflowStatus.ActivityTitle = pawsEntityDetails.ActivityTitle;
            currentWorkflowStatus.PseudoStateId = pawsEntityDetails.PseudoStateId;
            currentWorkflowStatus.PseudoStateTitle = pawsEntityDetails.PseudoStateTitle;
            currentWorkflowStatus.StatusId = pawsEntityDetails.StatusId;
            currentWorkflowStatus.StatusTitle = pawsEntityDetails.StatusTitle;
            currentWorkflowStatus.StepId = pawsEntityDetails.StepId;

            if (currentWorkflowStatus.StatusId == 1)
            {
                var activity = await pawsService.GetActivityAsync(currentWorkflowStatus.ActivityId, cancellationToken);
                var postRoles = await cbacService.GetRolesForPostAsync(debUser.PostId);
                var postRoleIds = (postRoles != null && postRoles.Count > 0) ? postRoles.Select(x => x.RoleID).ToList() : [];

                if (activity != null && activity.OwnerRoleIDs.Any())
                {
                    currentWorkflowStatus.CanApprove = activity.OwnerRoleIDs.Intersect(postRoleIds).Any();
                }

                if (currentWorkflowStatus.CanApprove)
                {
                    var pendingActivities = await pawsService.GetPendingActivitiesAsync(entityId, workflowId.Value, cancellationToken);

                    var selectedPendingActivity = pendingActivities.FirstOrDefault(x => x.ActivityID == currentWorkflowStatus.ActivityId);

                    currentWorkflowStatus.AvailableTriggerStates = selectedPendingActivity.AvailableTriggerStates;
                }
            }

            return currentWorkflowStatus;
        }


        [Authorize]
        public static async Task<ICollection<FilterItem>?> GetStandardVersionStatusLookup(
            IDebService debService,
            IPawsService pawsService,
            IApplicationSettingsService applicationSettingsService,
            CancellationToken cancellationToken)
            => await GetStatusLookup(EntityTypes.StandardVersion, debService, pawsService, applicationSettingsService, cancellationToken);

        [Authorize]
        public static async Task<ICollection<FilterItem>?> GetStatementStatusLookup(
            IDebService debService,
            IPawsService pawsService,
            IApplicationSettingsService applicationSettingsService,
            CancellationToken cancellationToken)
            => await GetStatusLookup(EntityTypes.SoC, debService, pawsService, applicationSettingsService, cancellationToken);

        [Authorize]
        public static async Task<ICollection<FilterItem>?> GetScopeStatusLookup(
            IDebService debService,
            IPawsService pawsService,
            IApplicationSettingsService applicationSettingsService,
            CancellationToken cancellationToken)
            => await GetStatusLookup(EntityTypes.Scope, debService, pawsService, applicationSettingsService, cancellationToken);

        [Authorize]
        public static async Task<ICollection<FilterItem>?> GetRequirementStatusLookup(
            IDebService debService,
            IPawsService pawsService,
            IApplicationSettingsService applicationSettingsService,
            CancellationToken cancellationToken)
            => await GetStatusLookup(EntityTypes.Requirement, debService, pawsService, applicationSettingsService, cancellationToken);

        [Authorize]
        public static async Task<ICollection<FilterItem>?> GetTaskStatusLookup(
            IDebService debService,
            IPawsService pawsService,
            IApplicationSettingsService applicationSettingsService,
            CancellationToken cancellationToken)
            => await GetStatusLookup(EntityTypes.Task, debService, pawsService, applicationSettingsService, cancellationToken);

        private static async Task<ICollection<FilterItem>?> GetStatusLookup(
            string entityType,
            IDebService debService,
            IPawsService pawsService,
            IApplicationSettingsService applicationSettingsService,
            CancellationToken cancellationToken)
        {
            var moduleId = applicationSettingsService.GetModuleId("DEB");

            var workflowId = await debService.GetWorkflowIdAsync(moduleId, entityType, cancellationToken);

            if (workflowId.HasValue == false)
            {
                throw new InvalidOperationException("WorkflowID could not be identified");
            }

            var pseudoStates = await pawsService.GetPseudoStatesByWorkflowAsync(workflowId.Value, cancellationToken);

            if (pseudoStates == null || pseudoStates.Count == 0)
                return null;

            var items = pseudoStates.Select(x => new FilterItem()
            {
                Id = x.PseudoStateID,
                Value = x.PseudoStateTitle,
                IsEnabled = true
            }).ToList();

            return items;
        }

        [Authorize]
        public static async Task<WorkflowHistory?> GetWorkflowHistoryAsync(
            Guid entityId,
            IDebService debService,
            IPawsService pawsService,
            IApplicationSettingsService applicationSettingsService,
            CancellationToken cancellationToken)
        {
            var moduleId = applicationSettingsService.GetModuleId("DEB");

            var entity = await debService.GetEntityHeadAsync(entityId, cancellationToken);

            if (entity == null)
            {
                throw new InvalidOperationException("EntityID could not be identified");
            }

            var workflowId = await debService.GetWorkflowIdAsync(moduleId, entity.EntityTypeTitle, cancellationToken);

            if (workflowId.HasValue == false)
            {
                throw new InvalidOperationException("WorkflowID could not be identified");
            }

            return await pawsService.GetWorkflowHistoryAsync(workflowId.Value, entityId, cancellationToken);
        }

        [Authorize]
        public static async Task<ICollection<WorkflowActivity>?> GetActivitiesForWorkflowAsync(
            string entityType,
            bool includeRemoved,
            IPawsService pawsService,
            IApplicationSettingsService applicationSettingsService,
            IDebService debService,
            CancellationToken cancellationToken)
        {
            var moduleId = applicationSettingsService.GetModuleId("DEB");

            var workflowId = await debService.GetWorkflowIdAsync(moduleId, entityType, cancellationToken);

            if (workflowId.HasValue == false)
            {
                throw new InvalidOperationException("WorkflowID could not be identified");
            }

            return await pawsService.GetActivitiesForWorkflowAsync(workflowId.Value, includeRemoved, cancellationToken);
        }
    }
}
