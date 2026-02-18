using Microsoft.Extensions.Logging;
using Nexus.DEB.Application.Common.Interfaces;
using Nexus.DEB.Domain.Models.Common;

namespace Nexus.DEB.Infrastructure.Services.DashboardProviders
{
    public class ScopeDashboardInfoProvider : DashboardInfoProviderBase
    {
        public ScopeDashboardInfoProvider(IPawsService pawsService, IDebService debService, IApplicationSettingsService applicationSettingsService, ILogger<ScopeDashboardInfoProvider> logger)
            : base(pawsService, debService, applicationSettingsService, logger, EntityTypes.Scope)
        {
        }
    }
}
