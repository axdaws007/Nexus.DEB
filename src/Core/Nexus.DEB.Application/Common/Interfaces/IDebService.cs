using Nexus.DEB.Application.Common.Models;
using Nexus.DEB.Application.Common.Models.Filters;
using Nexus.DEB.Domain.Models;

namespace Nexus.DEB.Application.Common.Interfaces
{
    public interface IDebService
    {
        #region Entity methods 

        IQueryable<RequirementSummary> GetRequirementsForGrid(RequirementSummaryFilters? filters);
        IQueryable<Scope> GetScopes();
        IQueryable<ScopeSummary> GetScopesForGrid();
        IQueryable<StandardVersion> GetStandardVersions();
        IQueryable<StandardVersionSummary> GetStandardVersionsForExportOrGrid(StandardVersionSummaryFilters? filters);
        IQueryable<StatementSummary> GetStatementsForGrid(StatementSummaryFilters? filters);
        IQueryable<TaskSummary> GetTasksForGrid(TaskSummaryFilters? filters);

        #endregion Entity methods

        #region Lookup methods

        IQueryable<Standard> GetStandards();
        IQueryable<TaskType> GetTaskTypes();

        #endregion
    }
}
