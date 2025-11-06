using Microsoft.Extensions.Logging;
using Nexus.DEB.Application.Common.Interfaces;
using Nexus.DEB.Application.Common.Models;
using Nexus.DEB.Domain.Models.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nexus.DEB.Infrastructure.Services
{
    public class WorkflowSideEffectService : IWorkflowSideEffectService
    {
        private readonly IDebService _debService;
        private readonly IPawsService _pawsService;
        private readonly ITransitionSideEffectRegistry _sideEffectRegistry;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<WorkflowSideEffectService> _logger;
        
        public WorkflowSideEffectService(
            IDebService debService,
            IPawsService pawsService,
            ITransitionSideEffectRegistry sideEffectRegistry,
            ICurrentUserService currentUserService,
            ILogger<WorkflowSideEffectService> logger)
        {
            _debService = debService;
            _pawsService = pawsService;
            _sideEffectRegistry = sideEffectRegistry;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<Result> ExecuteSideEffectAsync(Guid entityId, int stepId, int triggerStatusId, CancellationToken cancellationToken = default)
        {
            var entityHead = await _debService.GetEntityHeadAsync(entityId, cancellationToken);

            var destinationActivity = await _pawsService.GetDestinationActivitiesAsync(
                stepId, triggerStatusId, cancellationToken);

            if (destinationActivity == null)
            {
                return Result.Failure(new ValidationError
                {
                    Message = "Destination activity could not be found",
                    Code = "DESTINATION_NOT_FOUND"
                });
            }

            var sideEffectNames = destinationActivity.TargetActivities
                    .SelectMany(t => t.SideEffectTags)
                    .Distinct()
                    .ToList();

            // 3. Check if there are any validators configured
            if (sideEffectNames == null || !sideEffectNames.Any())
            {
                // No validation required
                _logger.LogInformation("No side effects configured for this transition");
                return Result.Success();
            }

            var allErrors = new List<ValidationError>();

            foreach (var targetActivity in destinationActivity.TargetActivities)
            {
                var context = new TransitionSideEffectContext
                {
                    EntityId = entityId,
                    EntityType = entityHead.EntityTypeTitle,
                    TriggerStatusId = triggerStatusId,
                    SourceActivityId = targetActivity.SourceActivityID,
                    DestinationActivityId = targetActivity.DestinationActivityID,
                    CurrentUserId = _currentUserService.UserId
                };

                var sideEffects = _sideEffectRegistry
                    .GetSideEffects(targetActivity.SideEffectTags)
                    .ToList();

                _logger.LogInformation(
                    "Running {Count} side effects: {Names}",
                    sideEffectNames.Count,
                    string.Join(", ", sideEffects.Select(v => v.Name)));

                foreach (var sideEffect in sideEffects)
                {
                    _logger.LogDebug($"Running side effect: {sideEffect.Name}");

                    var result = await sideEffect.ExecuteAsync(context, cancellationToken);

                    if (!result.IsSuccess)
                    {
                        _logger.LogWarning(
                            "Side Effect {Name} failed: {Errors}",
                            sideEffect.Name,
                            string.Join(", ", result.Errors.Select(e => e.Message)));

                        allErrors.AddRange(result.Errors);
                    }
                }
            }

            // 7. Return combined result
            if (allErrors.Any())
            {
                _logger.LogWarning(
                    "Transition side effect failed with {Count} error(s)",
                    allErrors.Count);

                return Result.Failure(allErrors);
            }

            _logger.LogInformation("Transition side effect executed");
            return Result.Success();
        }
    }
}
