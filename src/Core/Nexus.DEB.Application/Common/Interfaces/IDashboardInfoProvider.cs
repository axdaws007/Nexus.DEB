using Nexus.DEB.Domain.Models.Other;

namespace Nexus.DEB.Application.Common.Interfaces
{
    public interface IDashboardInfoProvider
    {
        string EntityType { get; }  // e.g., "Task", "Statement of Compliance"

        Task<DashboardInfo> CalculateDashboardInfoAsync(
            object entity,
            Guid entityId,
            CancellationToken cancellationToken = default);
    }
}
