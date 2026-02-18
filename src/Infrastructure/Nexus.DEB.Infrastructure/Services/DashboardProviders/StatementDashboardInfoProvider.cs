using Microsoft.Extensions.Logging;
using Nexus.DEB.Application.Common.Interfaces;
using Nexus.DEB.Application.Common.Models;
using Nexus.DEB.Domain.Models;
using Nexus.DEB.Domain.Models.Common;

namespace Nexus.DEB.Infrastructure.Services.DashboardProviders
{
    internal class StatementDashboardInfoProvider : DashboardInfoProviderBase
    {
        public StatementDashboardInfoProvider(IPawsService pawsService, IDebService debService, IApplicationSettingsService applicationSettingsService, ILogger<StatementDashboardInfoProvider> logger) 
            : base(pawsService, debService, applicationSettingsService, logger, EntityTypes.SoC)
        {
        }

        protected override DateOnly? DetermineReviewDate(object entity, Guid entityId)
        {
            if (entity is StatementDetail statementDetail)
            {
                return statementDetail.ReviewDate;
            }
            else if (entity is Statement statement)
            {
                return statement.ReviewDate;
            }

            return null;
        }
    }
}
