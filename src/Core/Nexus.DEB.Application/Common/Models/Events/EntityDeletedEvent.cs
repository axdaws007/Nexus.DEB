namespace Nexus.DEB.Application.Common.Models.Events
{
    public record EntityDeletedEvent : DomainEventBase
    {
        /// <summary>
        /// Optional: The entity before deletion (for audit data)
        /// </summary>
        public object? Entity { get; init; }

        public required string EntityType { get; init; }
        public required Guid EntityId { get; init; }
        public required string SerialNumber { get; init; }
        public required Guid UserId { get; init; }

        /// <summary>
        /// Whether this was a soft delete (true) or hard delete (false)
        /// </summary>
        public bool IsSoftDelete { get; init; } = true;

        public string EventContext => $"{EntityType} {SerialNumber} deleted.";
    }
}
