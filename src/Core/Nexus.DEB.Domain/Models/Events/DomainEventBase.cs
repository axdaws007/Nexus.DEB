using Nexus.DEB.Domain.Interfaces;

namespace Nexus.DEB.Domain.Models.Events
{
    public abstract record DomainEventBase : IDomainEvent
    {
        public DateTimeOffset OccurredAt { get; init; } = DateTimeOffset.UtcNow;
        public Guid CorrelationId { get; init; } = Guid.NewGuid();
    }
}
