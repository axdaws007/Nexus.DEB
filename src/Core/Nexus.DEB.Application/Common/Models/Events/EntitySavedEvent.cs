namespace Nexus.DEB.Application.Common.Models.Events
{
    public record EntitySavedEvent : DomainEventBase
    {
        /// <summary>
        /// The entity that was saved. Can be any detail type (TaskDetail, StatementDetail, ScopeDetail, etc.)
        /// </summary>
        public required object Entity { get; init; }

        /// <summary>
        /// The entity type from EntityTypes struct (e.g., "Task", "Statement of Compliance", "Scope")
        /// </summary>
        public required string EntityType { get; init; }

        /// <summary>
        /// The entity's primary key (EntityId)
        /// </summary>
        public required Guid EntityId { get; init; }

        /// <summary>
        /// The entity's serial number for audit message context
        /// </summary>
        public required string SerialNumber { get; init; }

        /// <summary>
        /// Whether this was a create (true) or update (false)
        /// </summary>
        public required bool IsNew { get; init; }

        /// <summary>
        /// Gets the event context message for audit logging.
        /// Format: "{EntityType} {SerialNumber} {created|updated}."
        /// </summary>
        public string EventContext => $"{EntityType} {SerialNumber} {(IsNew ? "created" : "updated")}.";
    }
}
