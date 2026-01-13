using Microsoft.Extensions.Logging;
using Nexus.DEB.Application.Common.Interfaces;
using Nexus.DEB.Application.Common.Models.Events;
using Nexus.DEB.Domain.Interfaces;

namespace Nexus.DEB.Infrastructure.Events.Subscribers
{
    public class WorkflowTransitionCompletedDashboardSubscriber : IDomainEventSubscriber<WorkflowTransitionCompletedEvent>
    {
        private readonly ILogger<WorkflowTransitionCompletedDashboardSubscriber> _logger;
        private readonly IDashboardInfoProviderRegistry _providerRegistry;
        private readonly IDebService _debService;

        public string Name => "WorkflowTransitionCompletedDashboard";

        // Run after audit logging
        public int Order => 50;

        public WorkflowTransitionCompletedDashboardSubscriber(
            ILogger<WorkflowTransitionCompletedDashboardSubscriber> logger,
            IDashboardInfoProviderRegistry providerRegistry,
            IDebService debService)
        {
            _logger = logger;
            _providerRegistry = providerRegistry;
            _debService = debService;
        }

        public async Task HandleAsync(WorkflowTransitionCompletedEvent @event, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation(
                "Processing workflow transition dashboard update for {EntityType} {SerialNumber} ({EntityId})",
                @event.EntityType,
                @event.SerialNumber,
                @event.EntityId);

            try
            {
                // Get the appropriate provider for this entity type
                var provider = _providerRegistry.GetProvider(@event.EntityType);

                if (provider == null)
                {
                    _logger.LogDebug(
                        "No dashboard provider registered for {EntityType}, skipping dashboard update",
                        @event.EntityType);
                    return;
                }

                // Calculate the dashboard info using entity-specific logic
                var dashboardInfo = await provider.CalculateDashboardInfoAsync(
                    @event.Entity,
                    @event.EntityId,
                    cancellationToken);

                // Persist to database (upsert)
                await _debService.UpsertDashboardInfoAsync(dashboardInfo, cancellationToken);

                _logger.LogDebug(
                    "Dashboard info updated for {EntityType} {EntityId}",
                    @event.EntityType,
                    @event.EntityId);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to update dashboard for {EntityType} {EntityId}",
                    @event.EntityType,
                    @event.EntityId);
            }
        }
    }
}
