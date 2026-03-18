using HotChocolate.Authorization;
using Nexus.DEB.Application.Common.Interfaces;
using Nexus.DEB.Application.Common.Models;
using Nexus.DEB.Application.Common.Models.Compliance;
using Nexus.DEB.Domain.Models;

namespace Nexus.DEB.Api.GraphQL
{
    [QueryType]
    public static class ComplianceQueries
    {
        [Authorize]
        public static async Task<ComplianceTreeResult> GetComplianceTree(
            Guid standardVersionId,
            Guid scopeId,
            List<int>? complianceStateFilter,
            bool hideEmptySections,
            IComplianceTreeService complianceTreeService,
            CancellationToken cancellationToken)
        {
            var query = new ComplianceTreeQuery
            {
                Tree = new TreeIdentifier(standardVersionId, scopeId),
                ComplianceStateFilter = complianceStateFilter,
                HideEmptySections = hideEmptySections
            };

            return await complianceTreeService.GetFilteredTreeAsync(query, cancellationToken);
        }

        [Authorize]
        public static async Task<IReadOnlyList<ComplianceState>> GetComplianceStates(
            IDebService debService,
            CancellationToken cancellationToken)
            => await debService.GetActiveComplianceStatesAsync(cancellationToken);
    }
}
