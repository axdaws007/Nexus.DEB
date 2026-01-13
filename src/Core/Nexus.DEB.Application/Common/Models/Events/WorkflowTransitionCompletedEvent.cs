namespace Nexus.DEB.Application.Common.Models.Events
{
    public record WorkflowTransitionCompletedEvent : DomainEventBase
    {
        public object? Entity { get; init; }
        public required Guid EntityId { get; init; }
        public required string EntityType { get; init; }
        public required string SerialNumber { get; init; }
        public required CurrentWorkflowStatus CurrentWorkflowStatus { get; init; }
        public required Guid WorkflowId { get; set; }

        public string EventContext => $"{EntityType} {SerialNumber} workflow update.";
    }
}
