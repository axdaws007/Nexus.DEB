using Nexus.DEB.Application.Common.Models;
using Nexus.DEB.Application.Common.Models.Filters;
using Nexus.DEB.Domain.Models;

namespace Nexus.DEB.Application.Common.Interfaces
{
    public interface IDebService
    {
        IQueryable<StandardVersionSummary> GetStandardVersionsForGrid(StandardVersionSummaryFilters? filters);
        IQueryable<ScopeSummary> GetScopesForGrid();
        IQueryable<RequirementSummary> GetRequirementsForGrid(RequirementSummaryFilters? filters);
        IQueryable<StatementSummary> GetStatementsForGrid(StatementSummaryFilters? filters);
        IQueryable<FilterItem<Guid>> GetScopesForFilter();
        IQueryable<FilterItem<Guid>> GetStandardVersionsForFilter();
        IQueryable<FilterItem<short>> GetStandardsForFilter();
    }
}
