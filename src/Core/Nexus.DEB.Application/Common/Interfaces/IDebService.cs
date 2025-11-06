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

        Task<ICollection<FilterItemEntity>> GetStandardVersionsLookupAsync(CancellationToken cancellationToken);
        Task<ICollection<FilterItemEntity>> GetScopesLookupAsync(CancellationToken cancellationToken);

        #endregion Entity methods

        #region Lookup methods

        IQueryable<Standard> GetStandards();
        Task<ICollection<FilterItem>> GetStandardsLookupAsync(CancellationToken cancellationToken);

        IQueryable<TaskType> GetTaskTypes();
        Task<ICollection<FilterItem>> GetTaskTypesLookupAsync(CancellationToken cancellationToken);

        #endregion

        #region Generic (eventual Framework)

        // Task<Guid?> GetWorkflowId(Guid moduleId, string entityType);

        #endregion Generic (eventual Framework)
    }
}
