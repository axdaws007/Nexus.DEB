using Microsoft.Extensions.Logging;
using Nexus.DEB.Application.Common.Extensions;
using Nexus.DEB.Application.Common.Interfaces;
using Nexus.DEB.Application.Common.Models;
using Nexus.DEB.Application.Common.Models.Events;
using Nexus.DEB.Domain;
using Nexus.DEB.Domain.Interfaces;

namespace Nexus.DEB.Infrastructure.Events.Subscribers
{
    public class WorkflowTransitionCompletedAuditSubscriber : IDomainEventSubscriber<WorkflowTransitionCompletedEvent>
    {
        private readonly ILogger<WorkflowTransitionCompletedAuditSubscriber> _logger;
        private readonly IAuditService _auditService;
        private readonly ICurrentUserService _currentUserService;
        private readonly IPawsService _pawsService;

        public string Name => "BS10008WorkflowTransitionCompletedAudit";

        // Run audit logging first (lower order = higher priority)
        public int Order => 10;

        public WorkflowTransitionCompletedAuditSubscriber(
            ILogger<WorkflowTransitionCompletedAuditSubscriber> logger,
            IAuditService auditService,
            ICurrentUserService currentUserService,
            IPawsService pawsService)
        {
            _logger = logger;
            _auditService = auditService;
            _currentUserService = currentUserService;
            _pawsService = pawsService;
        }

        public async Task HandleAsync(WorkflowTransitionCompletedEvent @event, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation(
                "Recording BS10008 workflow audit for {EntityType} {SerialNumber} ({EntityId})",
                @event.EntityType,
                @event.SerialNumber,
                @event.EntityId);

            try
            {
                var userDetails = await _currentUserService.GetUserDetailsAsync();

                var steps = await _pawsService.GetEntityActivityStepsAsync(@event.EntityId, @event.WorkflowId, cancellationToken);

                EntityActivityStep? lastSignedStep = null;
                if (steps != null && steps.Any())
                {
                    lastSignedStep = steps.Where(x => x.StatusID != DebHelper.Paws.Status.Pending).OrderByDescending(x => x.UpdatedDate).FirstOrDefault();
                }

                var data = new WorkflowAuditData
                {
                    CurrentWorkflowStatus = @event.CurrentWorkflowStatus,
                    LastSignedStep = lastSignedStep
                };

                // Use the existing ToAuditData extension method from JsonElementExtensions
                // This handles the serialization to JsonElement with type name
                var auditData = data.ToDeepAuditData("Workflow");

                // Call the existing IAuditService.EntitySaved method
                await _auditService.WorkflowSignoff(
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
