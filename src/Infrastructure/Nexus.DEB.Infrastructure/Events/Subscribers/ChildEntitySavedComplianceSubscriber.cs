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
        private readonly IComplianceTreeRebuildManager _rebuildManager;

        public string Name => "ChildEntitySavedCompliance";
        public int Order => 60;

        public ChildEntitySavedComplianceSubscriber(
            IDebService debService,
            ILogger<ChildEntitySavedComplianceSubscriber> logger,
            IComplianceTreeRebuildManager rebuildManager)
        {
            _debService = debService;
            _logger = logger;
            _rebuildManager = rebuildManager;
        }

        public async Task HandleAsync(
            ChildEntitySavedEvent @event,
            CancellationToken cancellationToken = default)
        {
            switch (@event.ParentEntityType)
            {
                case EntityTypes.StandardVersion:
                    _logger.LogInformation(
                        "Requesting compliance tree rebuilds for StandardVersion {StandardVersionId}, " +
                        "child type {ChildEntityType}: {EventContext}",
                        @event.ParentEntityId, @event.ChildEntityType, @event.EventContext);

                    try
                    {
                        await _rebuildManager.RequestAllTreeRebuildsForStandardVersionAsync(
                            @event.ParentEntityId, cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex,
                            "Failed to request compliance tree rebuilds for StandardVersion {StandardVersionId}",
                            @event.ParentEntityId);
                    }
                    break;

                case EntityTypes.Scope:
                    _logger.LogInformation(
                        "Requesting compliance tree rebuilds for Scope {ScopeId}, " +
                        "child type {ChildEntityType}: {EventContext}",
                        @event.ParentEntityId, @event.ChildEntityType, @event.EventContext);

                    try
                    {
                        var ids = await _debService.GetStandardVersionIdsByScopeAsync(
                            @event.ParentEntityId, cancellationToken);

                        foreach (var id in ids)
                        {
                            var tree = new TreeIdentifier(id, @event.ParentEntityId);
                            await _rebuildManager.RequestTreeRebuildAsync(tree, cancellationToken);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex,
                            "Failed to request compliance tree rebuilds for Scope {ScopeId}",
                            @event.ParentEntityId);
                    }
                    break;

                default:
                    _logger.LogDebug(
                        "ChildEntitySaved for parent type {ParentEntityType} not relevant " +
                        "to compliance tree, skipping",
                        @event.ParentEntityType);
                    break;
            }
        }
    }
}