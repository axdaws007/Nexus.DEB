using Nexus.DEB.Application.Common.Models;

namespace Nexus.DEB.Application.Common.Interfaces
{
    public interface IComplianceTreeService
    {
        Task<ComplianceTreeResult> GetFilteredTreeAsync(ComplianceTreeQuery query, CancellationToken cancellationToken = default);
    }
}
