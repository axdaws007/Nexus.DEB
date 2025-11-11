using HotChocolate.Authorization;
using Nexus.DEB.Application.Common.Interfaces;
using Nexus.DEB.Application.Common.Models;
using Nexus.DEB.Application.Common.Models.Filters;
using Nexus.DEB.Domain.Models;
using Nexus.DEB.Domain.Models.Common;

namespace Nexus.DEB.Api.GraphQL.Paws
{
    [QueryType]
    public static class WorkflowQueries
    {
        [Authorize]
        public static async Task<TransitionValidationResult> ValidateWorkflowTransition(
            Guid entityId,
            int stepId,
            int triggerStatusId,
            IWorkflowValidationService validationService,
            CancellationToken cancellationToken)
        {
            var result = await validationService.ValidateTransitionAsync(
                entityId,
                stepId,
                triggerStatusId,
                cancellationToken);

            return new TransitionValidationResult
            {
                CanProceed = result.IsSuccess,
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
        public static async Task<ICollection<FilterItem>?> GetStandardVersionStatusLookup(
            IDebService debService,
            IPawsService pawsService,
            IConfiguration configuration,
            CancellationToken cancellationToken)
            => await GetStatusLookup(EntityTypes.StandardVersion, debService, pawsService, configuration, cancellationToken);

        [Authorize]
        public static async Task<ICollection<FilterItem>?> GetStatementStatusLookup(
            IDebService debService,
            IPawsService pawsService,
            IConfiguration configuration,
            CancellationToken cancellationToken)
            => await GetStatusLookup(EntityTypes.SoC, debService, pawsService, configuration, cancellationToken);

        [Authorize]
        public static async Task<ICollection<FilterItem>?> GetScopeStatusLookup(
            IDebService debService,
            IPawsService pawsService,
            IConfiguration configuration,
            CancellationToken cancellationToken)
            => await GetStatusLookup(EntityTypes.Scope, debService, pawsService, configuration, cancellationToken);

        [Authorize]
        public static async Task<ICollection<FilterItem>?> GetRequirementStatusLookup(
            IDebService debService,
            IPawsService pawsService,
            IConfiguration configuration,
            CancellationToken cancellationToken)
            => await GetStatusLookup(EntityTypes.Requirement, debService, pawsService, configuration, cancellationToken);

        [Authorize]
        public static async Task<ICollection<FilterItem>?> GetTaskStatusLookup(
            IDebService debService,
            IPawsService pawsService,
            IConfiguration configuration,
            CancellationToken cancellationToken)
            => await GetStatusLookup(EntityTypes.Task, debService, pawsService, configuration, cancellationToken);

        private static async Task<ICollection<FilterItem>?> GetStatusLookup(
            string entityType,
            IDebService debService,
            IPawsService pawsService,
            IConfiguration configuration,
            CancellationToken cancellationToken)
        {
            var moduleIdString = configuration["Modules:DEB"] ?? throw new InvalidOperationException("Modules:DEB not configured in appsettings");

            if (!Guid.TryParse(moduleIdString, out var moduleId))
            {
                throw new InvalidOperationException("Modules:DEB must be a valid GUID");
            }

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

    }
}
