using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Nexus.DEB.Domain.Interfaces;

namespace Nexus.DEB.Infrastructure.Events
{
    public class DomainEventPublisher : IDomainEventPublisher
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<DomainEventPublisher> _logger;

        public DomainEventPublisher(
            IServiceProvider serviceProvider,
            ILogger<DomainEventPublisher> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
            where TEvent : IDomainEvent
        {
            var eventType = typeof(TEvent).Name;

            _logger.LogDebug(
                "Publishing event {EventType} with CorrelationId {CorrelationId}",
                eventType,
                @event.CorrelationId);

            // Get all subscribers for this event type from DI
            var subscribers = _serviceProvider
                .GetServices<IDomainEventSubscriber<TEvent>>()
                .OrderBy(s => s.Order)
                .ToList();

            if (subscribers.Count == 0)
            {
                _logger.LogDebug("No subscribers registered for event {EventType}", eventType);
                return;
            }

            _logger.LogDebug(
                "Found {Count} subscriber(s) for {EventType}: {Names}",
                subscribers.Count,
                eventType,
                string.Join(", ", subscribers.Select(s => s.Name)));

            foreach (var subscriber in subscribers)
            {
                await ExecuteSubscriberAsync(subscriber, @event, cancellationToken);
            }

            _logger.LogDebug(
                "Completed publishing event {EventType} with CorrelationId {CorrelationId}",
                eventType,
                @event.CorrelationId);
        }

        private async Task ExecuteSubscriberAsync<TEvent>(
            IDomainEventSubscriber<TEvent> subscriber,
            TEvent @event,
            CancellationToken cancellationToken) where TEvent : IDomainEvent
        {
            var eventType = typeof(TEvent).Name;

            try
            {
                _logger.LogDebug(
                    "Executing subscriber {SubscriberName} for event {EventType}",
                    subscriber.Name,
                    eventType);

                await subscriber.HandleAsync(@event, cancellationToken);

                _logger.LogDebug(
                    "Subscriber {SubscriberName} completed successfully for event {EventType}",
                    subscriber.Name,
                    eventType);
            }
            catch (Exception ex)
            {
                // Log but don't throw - other subscribers should still run
                _logger.LogError(
                    ex,
                    "Subscriber {SubscriberName} failed for event {EventType} with CorrelationId {CorrelationId}",
                    subscriber.Name,
                    eventType,
                    @event.CorrelationId);
            }
        }
    }
}
