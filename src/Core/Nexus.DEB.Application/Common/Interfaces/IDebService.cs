using Nexus.DEB.Application.Common.Models;
using Nexus.DEB.Application.Common.Models.Filters;
using Nexus.DEB.Domain.Models;

namespace Nexus.DEB.Application.Common.Interfaces
{
    public interface IDebService
    {
        IQueryable<StandardVersionSummary> GetStandardVersionsForGrid();
        IQueryable<ScopeSummary> GetScopesForGrid();
        IQueryable<RequirementSummary> GetRequirementsForGrid(RequirementSummaryFilters? filters);
        IQueryable<StatementSummary> GetStatementsForGrid(StatementSummaryFilters? filters);
        IQueryable<FilterItem> GetScopesForFilter();
        IQueryable<FilterItem> GetStandardVersionsForFilter();
    }
}
