namespace Nexus.DEB.Domain.Models.Events
{
    public record WorkflowTransitionCompletedEvent : DomainEventBase
    {
        public object? Entity { get; init; }
        public required Guid EntityId { get; init; }
        public required string EntityType { get; init; }
        public required string SerialNumber { get; init; }
        public required int SourceActivityId { get; init; }
        public required int DestinationActivityId { get; init; }
        public required int TriggerStatusId { get; init; }
        public required Guid UserId { get; init; }
        public string? Comments { get; init; }
    }
}
