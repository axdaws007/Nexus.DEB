using Microsoft.Extensions.Logging;
using Nexus.DEB.Application.Common.Interfaces;
using Nexus.DEB.Application.Common.Models;

namespace Nexus.DEB.Infrastructure.Services
{
    public class WorkflowValidationService : IWorkflowValidationService
    {
        private readonly IDebService _debService;
        private readonly ITransitionValidatorRegistry _validatorRegistry;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<WorkflowValidationService> _logger;

        public WorkflowValidationService(
            IDebService debService,
            ITransitionValidatorRegistry validatorRegistry,
            ICurrentUserService currentUserService,
            ILogger<WorkflowValidationService> logger)
        {
            _debService = debService;
            _validatorRegistry = validatorRegistry;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<Result> ValidateTransitionAsync(
            Guid entityId,
            int triggerStatusId,
            ICollection<TargetActivity>? targetActivities,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation(
                "Validating transition {TriggerStatusId} for {entityId}",
                triggerStatusId, entityId);

            var entityHead = await _debService.GetEntityHeadAsync(entityId, cancellationToken);

            // 1. Get current workflow state for the entity
            //var destinationActivity = await _pawsService.GetDestinationActivitiesAsync(
            //    stepId, triggerStatusId, cancellationToken);

            //if (destinationActivity == null)
            //{
            //    return Result.Failure(new ValidationError
            //    {
            //        Message = "Destination activity could not be found",
            //        Code = "DESTINATION_NOT_FOUND"
            //    });
            //}

            var validatorNames = targetActivities
                    .SelectMany(t => t.ValidatorTags)
                    .Distinct()
                    .ToList();

            // 3. Check if there are any validators configured
            if (validatorNames == null || !validatorNames.Any())
            {
                // No validation required
                _logger.LogInformation("No validators configured for this transition");
                return Result.Success();
            }

            var allErrors = new List<ValidationError>();

            foreach (var targetActivity in targetActivities)
            {
                var context = new TransitionValidationContext
                {
                    EntityId = entityId,
                    EntityType = entityHead.EntityTypeTitle,
                    TriggerStatusId = triggerStatusId,
                    SourceActivityId = targetActivity.SourceActivityID,
                    DestinationActivityId = targetActivity.DestinationActivityID,
                    CurrentUserId = _currentUserService.UserId
                };

                var validators = _validatorRegistry
                    .GetValidators(targetActivity.ValidatorTags)
                    .ToList();

                _logger.LogInformation(
                    "Running {Count} validators: {Names}",
                    validators.Count,
                    string.Join(", ", validators.Select(v => v.Name)));

                foreach (var validator in validators)
                {
                    _logger.LogDebug("Running validator: {ValidatorName}", validator.Name);

                    var result = await validator.ValidateAsync(context, cancellationToken);

                    if (!result.IsSuccess)
                    {
                        _logger.LogWarning(
                            "Validator {ValidatorName} failed: {Errors}",
                            validator.Name,
                            string.Join(", ", result.Errors.Select(e => e.Message)));

                        allErrors.AddRange(result.Errors);
                    }
                }
            }

            // 7. Return combined result
            if (allErrors.Any())
            {
                _logger.LogWarning(
                    "Transition validation failed with {Count} error(s)",
                    allErrors.Count);

                return Result.Failure(allErrors);
            }

            _logger.LogInformation("Transition validation passed");
            return Result.Success();
        }
    }
}
