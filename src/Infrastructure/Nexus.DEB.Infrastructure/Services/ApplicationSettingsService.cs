using Microsoft.Extensions.Configuration;
using Nexus.DEB.Application.Common.Interfaces;

namespace Nexus.DEB.Infrastructure.Services
{
    public class ApplicationSettingsService : IApplicationSettingsService
    {
        private readonly IConfiguration _configuration;

        public ApplicationSettingsService(IConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public Guid GetInstanceId()
        {
            return GetValueFromConfiguration<Guid>("InstanceId");
        }

        public Guid GetModuleId(string moduleName)
        {
            var appsettingIdentifier = $"Modules:{moduleName}";

            return GetValueFromConfiguration<Guid>(appsettingIdentifier);
        }

        private T GetValueFromConfiguration<T>(string appsettingIdentifier)
        {
            if (string.IsNullOrWhiteSpace(appsettingIdentifier))
                throw new ArgumentException("Appsetting identifier cannot be null or empty.", nameof(appsettingIdentifier));

            var value = _configuration[appsettingIdentifier];

            if (value == null)
                throw new KeyNotFoundException($"Configuration key '{appsettingIdentifier}' not found.");

            return (T)Convert.ChangeType(value, typeof(T));
        }
    }
}
