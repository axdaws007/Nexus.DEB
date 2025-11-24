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

            try
            {
                var targetType = typeof(T);
                var underlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;

                // Use TypeDescriptor which handles Guid, TimeSpan, and many other types
                var converter = System.ComponentModel.TypeDescriptor.GetConverter(underlyingType);

                if (converter.CanConvertFrom(typeof(string)))
                {
                    return (T)converter.ConvertFromInvariantString(value)!;
                }

                // Fallback to Convert.ChangeType for basic types
                return (T)Convert.ChangeType(value, underlyingType);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Failed to convert configuration value '{value}' for key '{appsettingIdentifier}' to type {typeof(T).Name}.", ex);
            }
        }
    }
}
