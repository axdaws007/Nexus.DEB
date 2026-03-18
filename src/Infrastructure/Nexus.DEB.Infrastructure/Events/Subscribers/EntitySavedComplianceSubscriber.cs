using Microsoft.Extensions.Logging;
using Nexus.DEB.Application.Common.Interfaces;
using Nexus.DEB.Application.Common.Models.Compliance;
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
        private readonly IComplianceTreeRecalculator _recalculator;
        private readonly IDebService _debService;

        public string Name => "EntitySavedCompliance";
        public int Order => 60;

        public EntitySavedComplianceSubscriber(
            ILogger<EntitySavedComplianceSubscriber> logger,
            IComplianceTreeRecalculator recalculator,
            IDebService debService)
        {
            _logger = logger;
            _recalculator = recalculator;
            _debService = debService;
        }

        public async Task HandleAsync(
            EntitySavedEvent @event,
            CancellationToken cancellationToken = default)
        {
            try
            {
                switch (@event.EntityType)
                {
                    case EntityTypes.SoC:
                        await HandleStatementSavedAsync(@event, cancellationToken);
                        break;

                    default:
                        return;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to update compliance tree for {EntityType} {EntityId}",
                    @event.EntityType, @event.EntityId);
            }
        }

        private async Task HandleStatementSavedAsync(
            EntitySavedEvent @event,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                "Processing statement {Action} compliance update for {EntityId}",
                @event.IsNew ? "creation" : "update", @event.EntityId);

            // 1. Trees that currently contain this Statement (covers removals on update)
            var existingTrees = await _debService.GetTreesContainingEntityAsync(
                @event.EntityId, ComplianceNodeTypes.Statement, cancellationToken);

            // 2. Trees that should contain this Statement (from current SRS links)
            var expectedTrees = await _debService.GetTreeIdentifiersForStatementAsync(
                @event.EntityId, cancellationToken);

            // 3. Union — rebuild all affected trees
            var allTrees = existingTrees
                .Union(expectedTrees)
                .Distinct()
                .ToList();

            if (allTrees.Count == 0)
            {
                _logger.LogDebug(
                    "No compliance trees affected by Statement {EntityId}, skipping",
                    @event.EntityId);
                return;
            }

            _logger.LogDebug(
                "Rebuilding {Count} compliance tree(s) for Statement {EntityId}",
                allTrees.Count, @event.EntityId);

            foreach (var tree in allTrees)
            {
                try
                {
                    await _recalculator.RebuildTreeAsync(tree, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "Failed to rebuild compliance tree for SV={StandardVersionId} Scope={ScopeId}",
                        tree.StandardVersionId, tree.ScopeId);
                }
            }
        }
    }
}