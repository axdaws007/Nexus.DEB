using Microsoft.Extensions.Configuration;
using Nexus.DEB.Application.Common.Interfaces;
using Nexus.DEB.Application.Common.Models;

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

        public Guid GetLibraryId(string libraryName)
        {
            var libraryIdentifier = $"DMS:Libraries:{libraryName}";

            return _configuration.GetValue<Guid>(libraryIdentifier);
        }

        public AuditConfiguration GetAuditConfiguration()
        {
            return new AuditConfiguration()
            {
                PlatformTeam = _configuration.GetValue<string>("Audit:PlatformTeam", string.Empty),
                ApplicationName = _configuration.GetValue<string>("Audit:ApplicationName", string.Empty),
                ApplicationInstance = _configuration.GetValue<string>("Audit:ApplicationInstance", string.Empty),
                EnvironmentName = _configuration.GetValue<string>("Audit:EnvironmentName")
            };
        }
    }
}
