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
            return _configuration.GetValue<Guid>("InstanceId");
        }

        public Guid GetModuleId(string moduleName)
        {
            var appsettingIdentifier = $"Modules:{moduleName}";

            return _configuration.GetValue<Guid>(appsettingIdentifier);
        }
    }
}
