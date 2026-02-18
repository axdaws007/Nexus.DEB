using Microsoft.Extensions.Logging;
using Nexus.DEB.Application.Common.Interfaces;
using Nexus.DEB.Domain.Models.Common;

namespace Nexus.DEB.Infrastructure.Services.DashboardProviders
{
    public class RequirementDashboardInfoProvider : DashboardInfoProviderBase
    {
        public RequirementDashboardInfoProvider(IPawsService pawsService, IDebService debService, IApplicationSettingsService applicationSettingsService, ILogger<RequirementDashboardInfoProvider> logger) 
            : base(pawsService, debService, applicationSettingsService, logger, EntityTypes.Requirement)
        {
        }
    }
}
