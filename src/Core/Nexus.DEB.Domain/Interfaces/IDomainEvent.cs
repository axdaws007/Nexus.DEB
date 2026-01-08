namespace Nexus.DEB.Domain.Interfaces
{
    public interface IDomainEvent
    {
        /// <summary>
        /// When the event occurred (UTC)
        /// </summary>
        DateTimeOffset OccurredAt { get; }

        /// <summary>
        /// Correlation ID for tracing across subscribers
        /// </summary>
        Guid CorrelationId { get; }
    }
}
