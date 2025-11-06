namespace Nexus.DEB.Application.Common.Models
{
    public class TransitionValidationContext
    {
        public Guid EntityId { get; init; }
        public string EntityType { get; init; } = string.Empty;
        public int TriggerStatusId { get; init; }
        public int SourceActivityId { get; init; }
        public int DestinationActivityId { get; init; }
        public Guid CurrentUserId { get; init; }
    }
}
