using Microsoft.Extensions.Logging;
using Nexus.DEB.Application.Common.Interfaces;

namespace Nexus.DEB.Infrastructure.Services.Registries
{
    public class DashboardInfoProviderRegistry : IDashboardInfoProviderRegistry
    {
        private readonly Dictionary<string, IDashboardInfoProvider> _providers;
        private readonly ILogger<DashboardInfoProviderRegistry> _logger;

        public DashboardInfoProviderRegistry(
            IEnumerable<IDashboardInfoProvider> providers,
            ILogger<DashboardInfoProviderRegistry> logger)
        {
            _logger = logger;

            // Build dictionary of providers by entity type
            _providers = providers.ToDictionary(
                p => p.EntityType,
                p => p,
                StringComparer.OrdinalIgnoreCase);

            _logger.LogInformation(
                "Registered {Count} dashboard info provider(s): {EntityTypes}",
                _providers.Count,
                string.Join(", ", _providers.Keys));
        }

        public IDashboardInfoProvider? GetProvider(string entityType)
        {
            if (_providers.TryGetValue(entityType, out var provider))
            {
                return provider;
            }

            _logger.LogWarning(
                "No dashboard info provider registered for entity type '{EntityType}'",
                entityType);

            return null;
        }

        public IEnumerable<string> GetRegisteredEntityTypes() => _providers.Keys;
    }
}
