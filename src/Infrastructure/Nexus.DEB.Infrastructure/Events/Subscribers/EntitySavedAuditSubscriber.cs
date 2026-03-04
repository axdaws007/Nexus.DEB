using Microsoft.Extensions.Logging;
using Nexus.DEB.Application.Common.Extensions;
using Nexus.DEB.Application.Common.Interfaces;
using Nexus.DEB.Application.Common.Models;
using Nexus.DEB.Application.Common.Models.Events;
using Nexus.DEB.Domain.Interfaces;

namespace Nexus.DEB.Infrastructure.Events.Subscribers
{
    public class EntitySavedAuditSubscriber : IDomainEventSubscriber<EntitySavedEvent>, IDomainEventSubscriber<ChildEntitySavedEvent>
    {
        private readonly ILogger<EntitySavedAuditSubscriber> _logger;
        private readonly IAuditService _auditService;
        private readonly ICurrentUserService _currentUserService;
        private readonly IDebService _debService;

        public string Name => "BS10008EntitySavedAudit";

        // Run audit logging first (lower order = higher priority)
        public int Order => 10;

        public EntitySavedAuditSubscriber(
            ILogger<EntitySavedAuditSubscriber> logger,
            IAuditService auditService,
            ICurrentUserService currentUserService,
            IDebService debService)
        {
            _logger = logger;
            _auditService = auditService;
            _currentUserService = currentUserService;
            _debService = debService;
        }

        public async Task HandleAsync(EntitySavedEvent @event, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation(
                "Recording BS10008 audit for EntitySaved event on {EntityType} {SerialNumber} ({EntityId})",
                @event.EntityType,
                @event.SerialNumber,
                @event.EntityId);

            await PublishAuditEvent(
                @event.EntityId,
                @event.EntityType,
                @event.EventContext,
                @event.Entity);
        }

        public async Task HandleAsync(ChildEntitySavedEvent @event, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation(
                "Recording BS10008 audit for ChildEntitySaved event for parent entity {ParentEntityType} ({ParentEntityId}), child entity is {ChildEntityType}",
                @event.ParentEntityType,
                @event.ParentEntityId,
                @event.ChildEntityType);

            await PublishAuditEvent(
                @event.ParentEntityId,
                @event.ParentEntityType,
                @event.EventContext,
                null);
        }

        private async Task PublishAuditEvent(Guid entityId, string entityType, string eventContext, object? entity)
        {

            try
            {
                var userDetails = await _currentUserService.GetUserDetailsAsync();

                // Option 1 - Try and get the data using a snapshot SP
                AuditData? auditData = await _debService.GetAuditDataAsync(entityId, entityType);

                if (auditData == null && entity != null)
                {
                    //If option 1 isn't successful, use the data provided in the event.
                    // Use the existing ToAuditData extension method from JsonElementExtensions
                    // This handles the serialization to JsonElement with type name
                    auditData = entity.ToAuditData(entityType);
                }

                // Call the existing IAuditService.EntitySaved method
                await _auditService.EntitySaved(
                    entityId,
                    entityType,
                    eventContext,    // e.g., "Task TSK-000001 updated."
                    userDetails,
                    auditData);

                _logger.LogDebug(
                    "BS10008 audit recorded successfully for {EntityType} {EntityId}",
                    entityType,
                    entityId);
            }
            catch (Exception ex)
            {
                // Log error but don't throw - other subscribers should still run
                _logger.LogError(
                    ex,
                    "Failed to record BS10008 audit for {EntityType} {EntityId}",
                    entityType,
                    entityId);
            }
        }
    }
}
