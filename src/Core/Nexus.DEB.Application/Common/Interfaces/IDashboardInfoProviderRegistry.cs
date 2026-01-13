namespace Nexus.DEB.Application.Common.Interfaces
{
    public interface IDashboardInfoProviderRegistry
    {
        /// <summary>
        /// Gets the provider for the specified entity type.
        /// </summary>
        /// <param name="entityType">The entity type (from EntityTypes constants)</param>
        /// <returns>The provider, or null if no provider is registered for this entity type</returns>
        IDashboardInfoProvider? GetProvider(string entityType);

        /// <summary>
        /// Gets all registered entity types (for diagnostics).
        /// </summary>
        IEnumerable<string> GetRegisteredEntityTypes();
    }
}
