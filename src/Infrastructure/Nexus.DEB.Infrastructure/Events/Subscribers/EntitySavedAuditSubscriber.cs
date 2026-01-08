using Microsoft.Extensions.Logging;
using Nexus.DEB.Application.Common.Extensions;
using Nexus.DEB.Application.Common.Interfaces;
using Nexus.DEB.Domain.Interfaces;
using Nexus.DEB.Domain.Models.Events;

namespace Nexus.DEB.Infrastructure.Events.Subscribers
{
    public class EntitySavedAuditSubscriber : IDomainEventSubscriber<EntitySavedEvent>
    {
        private readonly ILogger<EntitySavedAuditSubscriber> _logger;
        private readonly IAuditService _auditService;
        private readonly ICurrentUserService _currentUserService;

        public string Name => "BS10008EntitySavedAudit";

        // Run audit logging first (lower order = higher priority)
        public int Order => 10;

        public EntitySavedAuditSubscriber(
            ILogger<EntitySavedAuditSubscriber> logger,
            IAuditService auditService,
            ICurrentUserService currentUserService)
        {
            _logger = logger;
            _auditService = auditService;
            _currentUserService = currentUserService;
        }

        public async Task HandleAsync(EntitySavedEvent @event, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation(
                "Recording BS10008 audit for {EntityType} {SerialNumber} ({EntityId}) - IsNew: {IsNew}",
                @event.EntityType,
                @event.SerialNumber,
                @event.EntityId,
                @event.IsNew);

            try
            {
                var userDetails = await _currentUserService.GetUserDetailsAsync();

                // Use the existing ToAuditData extension method from JsonElementExtensions
                // This handles the serialization to JsonElement with type name
                var auditData = @event.Entity.ToAuditData(@event.EntityType);

                // Call the existing IAuditService.EntitySaved method
                await _auditService.EntitySaved(
                    @event.EntityId,
                    @event.EntityType,
                    @event.EventContext,    // e.g., "Task TSK-000001 updated."
                    userDetails,
                    auditData);

                _logger.LogDebug(
                    "BS10008 audit recorded successfully for {EntityType} {EntityId}",
                    @event.EntityType,
                    @event.EntityId);
            }
            catch (Exception ex)
            {
                // Log error but don't throw - other subscribers should still run
                _logger.LogError(
                    ex,
                    "Failed to record BS10008 audit for {EntityType} {EntityId}",
                    @event.EntityType,
                    @event.EntityId);
            }
        }
    }
}
