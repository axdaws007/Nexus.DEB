using Microsoft.Extensions.Logging;
using Nexus.DEB.Application.Common.Extensions;
using Nexus.DEB.Application.Common.Interfaces;
using Nexus.DEB.Application.Common.Models.Events;
using Nexus.DEB.Domain.Interfaces;

namespace Nexus.DEB.Infrastructure.Events.Subscribers
{
    public class EntityDeletedAuditSubscriber : IDomainEventSubscriber<EntityDeletedEvent>
    {
        private readonly ILogger<EntityDeletedAuditSubscriber> _logger;
        private readonly IAuditService _auditService;
        private readonly ICurrentUserService _currentUserService;

        public string Name => "BS10008EntityDeletedAudit";
        public int Order => 10;

        public EntityDeletedAuditSubscriber(
            ILogger<EntityDeletedAuditSubscriber> logger,
            IAuditService auditService,
            ICurrentUserService currentUserService)
        {
            _logger = logger;
            _auditService = auditService;
            _currentUserService = currentUserService;
        }

        public async Task HandleAsync(EntityDeletedEvent @event, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation(
                "Recording BS10008 audit for deleted {EntityType} {SerialNumber} ({EntityId})",
                @event.EntityType,
                @event.SerialNumber,
                @event.EntityId);

            try
            {
                var userDetails = await _currentUserService.GetUserDetailsAsync();

                // If entity was provided, include it in audit data
                var auditData = @event.Entity?.ToAuditData(@event.EntityType);

                await _auditService.EntityDeleted(
                    @event.EntityId,
                    @event.EntityType,
                    @event.EventContext,
                    userDetails,
                    auditData);

                _logger.LogDebug(
                    "BS10008 delete audit recorded for {EntityType} {EntityId}",
                    @event.EntityType,
                    @event.EntityId);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to record BS10008 delete audit for {EntityType} {EntityId}",
                    @event.EntityType,
                    @event.EntityId);
            }
        }
    }
}
