namespace Nexus.DEB.Domain.Interfaces
{
    public interface IDomainEventPublisher
    {
        /// <summary>
        /// Publishes an event to all registered subscribers.
        /// Subscribers are executed in order (by Order property).
        /// Subscriber exceptions are caught and logged but don't affect other subscribers.
        /// </summary>
        /// <typeparam name="TEvent">The type of event to publish</typeparam>
        /// <param name="event">The event to publish</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task that completes when all subscribers have finished</returns>
        Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default) where TEvent : IDomainEvent;
    }
}
