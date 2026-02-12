using Nexus.DEB.Application.Common.Interfaces;
using Nexus.DEB.Domain.Models.Common;

namespace Nexus.DEB.Infrastructure.Services.DashboardProviders
{
    public class StandardVersionDashboardInfoProvider : DashboardInfoProviderBase
    {
        public StandardVersionDashboardInfoProvider(IPawsService pawsService, IDebService debService, IApplicationSettingsService applicationSettingsService)
            : base(pawsService, debService, applicationSettingsService, EntityTypes.StandardVersion)
        {
        }
    }
}
