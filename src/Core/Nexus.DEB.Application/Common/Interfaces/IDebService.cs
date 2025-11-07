using Nexus.DEB.Application.Common.Models;
using Nexus.DEB.Application.Common.Models.Filters;
using Nexus.DEB.Domain.Models;
using Nexus.DEB.Domain.Models.Common;

namespace Nexus.DEB.Application.Common.Interfaces
{
    public interface IDebService
    {
        #region Entity methods 
        Task<EntityHead?> GetEntityHeadAsync(Guid id, CancellationToken cancellationToken);
        IQueryable<RequirementSummary> GetRequirementsForGrid(RequirementSummaryFilters? filters);
        IQueryable<RequirementExport> GetRequirementsForExport(RequirementSummaryFilters? filters);
        IQueryable<Requirement> GetRequirementsForStandardVersion(Guid standardVersionId);
        IQueryable<Scope> GetScopes();
        IQueryable<ScopeSummary> GetScopesForGrid();
        IQueryable<ScopeExport> GetScopesForExport();
        Task<StandardVersion?> GetStandardVersionByIdAsync(Guid id, CancellationToken cancellationToken);
        IQueryable<StandardVersion> GetStandardVersions();
        IQueryable<StandardVersionSummary> GetStandardVersionsForGrid(StandardVersionSummaryFilters? filters);
        IQueryable<StandardVersionExport> GetStandardVersionsForExport(StandardVersionSummaryFilters? filters);
        IQueryable<StatementSummary> GetStatementsForGrid(StatementSummaryFilters? filters);
        IQueryable<TaskSummary> GetTasksForGrid(TaskSummaryFilters? filters);
        IQueryable<TaskExport> GetTasksForExport(TaskSummaryFilters? filters);

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

        Task<Guid?> GetWorkflowIdAsync(Guid moduleId, string entityType, CancellationToken cancellationToken);

        #endregion Generic (eventual Framework)
    }
}
