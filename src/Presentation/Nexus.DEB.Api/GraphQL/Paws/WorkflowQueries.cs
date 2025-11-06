using HotChocolate.Authorization;
using Nexus.DEB.Application.Common.Models.Filters;
using Nexus.DEB.Api.GraphQL.Paws.Models;
using Nexus.DEB.Application.Common.Interfaces;
using Nexus.DEB.Application.Common.Models;
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
        public static async Task<ICollection<FilterItem>?> GetStandardVersionStatusLookup(
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

            var workflowId = await debService.GetWorkflowIdAsync(moduleId, EntityTypes.StandardVersion, cancellationToken);

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
