namespace Nexus.DEB.Application.Common.Models.Events
{
    public record ChildEntitySavedEvent : DomainEventBase
    {
        public required string ParentEntityType { get; init; }
        public required Guid ParentEntityId { get; init; }
        public required string ChildEntityType { get; init; } // e.g. "Section"
        public required string EventContext { get; init; }
    }
}
