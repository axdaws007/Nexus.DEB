using Microsoft.Extensions.Logging;
using Nexus.DEB.Application.Common.Interfaces;
using Nexus.DEB.Application.Common.Models.Events;
using Nexus.DEB.Domain.Interfaces;
using Nexus.DEB.Domain.Models;
using Nexus.DEB.Domain.Models.Common;
using Task = System.Threading.Tasks.Task;

namespace Nexus.DEB.Infrastructure.Events.Subscribers
{
    public class EntitySavedComplianceSubscriber
        : IDomainEventSubscriber<EntitySavedEvent>
    {
        private readonly ILogger<EntitySavedComplianceSubscriber> _logger;
        private readonly IComplianceTreeRebuildManager _rebuildManager;
        private readonly IDebService _debService;

        public string Name => "EntitySavedCompliance";
        public int Order => 60;

        public EntitySavedComplianceSubscriber(
            ILogger<EntitySavedComplianceSubscriber> logger,
            IComplianceTreeRebuildManager rebuildManager,
            IDebService debService)
        {
            _logger = logger;
            _rebuildManager = rebuildManager;
            _debService = debService;
        }

        public async Task HandleAsync(
            EntitySavedEvent @event,
            CancellationToken cancellationToken = default)
        {
            // Only Statements need tree rebuilds on save.
            // New Statements need adding to trees, updated Statements may
            // have changed their requirement-scope links.
            // Workflow state changes are handled by WorkflowTransitionCompletedComplianceSubscriber.
            if (@event.EntityType != EntityTypes.SoC)
                return;

            _logger.LogInformation(
                "Requesting compliance tree rebuilds for Statement {StatementId} " +
                "(IsNew={IsNew})",
                @event.EntityId, @event.IsNew);

            try
            {
                // Find trees the Statement currently lives in (handles removals)
                var existingTrees = await _debService.GetTreesContainingEntityAsync(
                    @event.EntityId, ComplianceNodeTypes.Statement, cancellationToken);

                // Find trees the Statement should live in (handles additions)
                var targetTrees = await _debService.GetTreeIdentifiersForStatementAsync(
                    @event.EntityId, cancellationToken);

                // Union both sets
                var allAffectedTrees = existingTrees
                    .Union(targetTrees)
                    .Distinct()
                    .ToList();

                if (allAffectedTrees.Count == 0)
                {
                    _logger.LogDebug(
                        "No affected trees found for Statement {StatementId}, skipping",
                        @event.EntityId);
                    return;
                }

                foreach (var tree in allAffectedTrees)
                {
                    await _rebuildManager.RequestTreeRebuildAsync(tree, cancellationToken);
                }

                _logger.LogInformation(
                    "Requested {Count} compliance tree rebuild(s) for Statement {StatementId}",
                    allAffectedTrees.Count, @event.EntityId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to request compliance tree rebuilds for Statement {StatementId}",
                    @event.EntityId);
            }
        }
    }
}