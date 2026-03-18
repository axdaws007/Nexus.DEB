using Microsoft.Extensions.Logging;
using Nexus.DEB.Application.Common.Interfaces;
using Nexus.DEB.Application.Common.Models.Compliance;
using Nexus.DEB.Application.Common.Models.Events;
using Nexus.DEB.Domain.Interfaces;
using Nexus.DEB.Domain.Models.Common;
using Task = System.Threading.Tasks.Task;

namespace Nexus.DEB.Infrastructure.Events.Subscribers
{
    public class ChildEntitySavedComplianceSubscriber
        : IDomainEventSubscriber<ChildEntitySavedEvent>
    {
        private readonly ILogger<ChildEntitySavedComplianceSubscriber> _logger;
        private readonly IDebService _debService;
        private readonly IComplianceTreeRecalculator _recalculator;

        public string Name => "ChildEntitySavedCompliance";
        public int Order => 60;

        public ChildEntitySavedComplianceSubscriber(
            IDebService debService,
            ILogger<ChildEntitySavedComplianceSubscriber> logger,
            IComplianceTreeRecalculator recalculator)
        {
            _debService = debService;
            _logger = logger;
            _recalculator = recalculator;
        }

        public async Task HandleAsync(
            ChildEntitySavedEvent @event,
            CancellationToken cancellationToken = default)
        {
            if (@event.ParentEntityType == EntityTypes.StandardVersion)
            {
                _logger.LogInformation(
                    "Processing structural change compliance update for StandardVersion {StandardVersionId}, " +
                    "child type {ChildEntityType}: {EventContext}",
                    @event.ParentEntityId, @event.ChildEntityType, @event.EventContext);


                try
                {
                    // Rebuild all trees for this Standard Version (across all Scopes)
                    // This is the safest approach for structural changes — section moves,
                    // requirement reassignments, etc. can affect the tree shape in ways
                    // that are complex to handle differentially.
                    await _recalculator.RebuildAllTreesForStandardVersionAsync(@event.ParentEntityId, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to rebuild compliance trees for StandardVersion {StandardVersionId}", @event.ParentEntityId);
                }
            }

            else if (@event.ParentEntityType == EntityTypes.Scope)
            {
                _logger.LogInformation(
                    "Processing structural change compliance update for Scope {ScopeId}, " +
                    "child type {ChildEntityType}: {EventContext}",
                    @event.ParentEntityId, @event.ChildEntityType, @event.EventContext);

                var ids = await _debService.GetStandardVersionIdsByScopeAsync(@event.ParentEntityId, cancellationToken);

                foreach(var id in ids)
                {
                    _logger.LogDebug("Processing tree for StandardVersion {StandardVersionId} and Scope {ScopeId}", id, @event.ParentEntityId);

                    var tree = new TreeIdentifier(id, @event.ParentEntityId);

                    try
                    {
                        await _recalculator.RebuildTreeAsync(tree, cancellationToken);
                    }
                    catch(Exception ex)
                    {
                        _logger.LogError(ex, "Failed to rebuild compliance tree for StandardVersion {StandardVersionId} and Scope {ScopeId}", id, @event.ParentEntityId);
                    }
                }
            }

            else
            {
                _logger.LogDebug("ChildEntitySaved for parent type {ParentEntityType} not relevant to compliance tree, skipping", @event.ParentEntityType);
            }

            return;
        }
    }
}