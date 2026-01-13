using Nexus.DEB.Application.Common.Interfaces;
using Nexus.DEB.Application.Common.Models;
using Nexus.DEB.Domain.Models.Common;

namespace Nexus.DEB.Infrastructure.Services.DashboardProviders
{
    internal class StatementDashboardInfoProvider : DashboardInfoProviderBase
    {
        public StatementDashboardInfoProvider(IPawsService pawsService, IDebService debService, IApplicationSettingsService applicationSettingsService) 
            : base(pawsService, debService, applicationSettingsService, EntityTypes.SoC)
        {
        }

        protected override DateTime? DetermineReviewDate(object entity, Guid entityId)
        {
            if (entity is StatementDetail statementDetail)
            {
                return statementDetail.ReviewDate;
            }

            return null;
        }
    }
}
