namespace Nexus.DEB.Domain.Interfaces
{
    public interface IDomainEventSubscriber<in TEvent> where TEvent : IDomainEvent
    {
        /// <summary>
        /// Unique name for logging and diagnostics
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Execution order when multiple subscribers exist for the same event.
        /// Lower values run first. Default is 100.
        /// </summary>
        int Order => 100;

        /// <summary>
        /// Handle the event. Exceptions are caught and logged but don't affect other subscribers.
        /// </summary>
        Task HandleAsync(TEvent @event, CancellationToken cancellationToken = default);
    }
}
