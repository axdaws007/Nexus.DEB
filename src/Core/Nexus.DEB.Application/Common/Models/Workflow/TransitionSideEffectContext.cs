namespace Nexus.DEB.Application.Common.Models.Workflow
{
    public class TransitionSideEffectContext
    {
        public Guid EntityId { get; init; }
        public string EntityType { get; init; }
        public int TriggerStatusId { get; init; }
        public int SourceActivityId { get; init; }
        public int DestinationActivityId { get; init; }
        public string NewStatus { get; init; }
        public Guid CurrentUserId { get; init; }
        public DateTime TransitionedAt { get; init; }
        public string? Comments { get; init; }
        public Dictionary<string, object> Metadata { get; init; }
    }
}
