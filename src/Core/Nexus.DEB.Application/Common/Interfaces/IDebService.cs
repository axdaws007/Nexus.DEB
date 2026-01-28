using Nexus.DEB.Application.Common.Models;
using Nexus.DEB.Application.Common.Models.Filters;
using Nexus.DEB.Domain.Models;
using Nexus.DEB.Domain.Models.Common;
using Nexus.DEB.Domain.Models.Other;
using Task = System.Threading.Tasks.Task;

namespace Nexus.DEB.Application.Common.Interfaces
{
    public interface IDebService
    {
        #region Entity methods 
        Task<EntityHead?> GetEntityHeadAsync(Guid id, CancellationToken cancellationToken);
        Task<EntityHeadDetail?> GetEntityHeadDetailAsync(Guid id, CancellationToken cancellationToken);
        Task<Dictionary<Guid, EntityHead>> GetEntityHeadsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken);
        IQueryable<RequirementSummary> GetRequirementsForGrid(RequirementSummaryFilters? filters);
        IQueryable<StandardVersionRequirementDetail> GetStandardVersionRequirementsForGrid(StandardVersionRequirementsFilters? filters);

		IQueryable<RequirementExport> GetRequirementsForExport(RequirementSummaryFilters? filters);
		Task<RequirementDetail?> GetRequirementByIdAsync(Guid id, CancellationToken cancellationToken);
        ICollection<StandardVersionWithSections> GetRelatedStandardVersionsAndSections(Guid requirementId);
		ICollection<ScopeWithStatements> GetRelatedScopesWithStatements(Guid requirementId);
		Task<RequirementChildCounts> GetChildCountsForRequirementAsync(Guid id, CancellationToken cancellationToken);
		IQueryable<Requirement> GetRequirementsForStandardVersion(Guid standardVersionId);
        Task<ICollection<RequirementWithScopes>> GetRequirementScopesForStatement(Guid statementId, CancellationToken cancellationToken);
        Task<List<StatementRequirementScope>> GetRequirementScopeCombinations(IEnumerable<(Guid RequirementId, Guid ScopeId)> combinations, CancellationToken cancellationToken);
        IQueryable<Scope> GetScopes();
        IQueryable<ScopeSummary> GetScopesForGrid();
        IQueryable<ScopeExport> GetScopesForExport();
		Task<Scope?> GetScopeByIdAsync(Guid id, CancellationToken cancellationToken);
		Task<ScopeDetail?> GetScopeDetailByIdAsync(Guid id, CancellationToken cancellationToken);
		Task<ScopeChildCounts> GetChildCountsForScopeAsync(Guid id, CancellationToken cancellationToken);
		Task<ICollection<ScopeCondensed>> GetScopesForRequirementAsync(Guid requirementId, Guid? statementId, CancellationToken cancellationToken);
		Task<Scope> CreateScopeAsync(Scope scope, CancellationToken cancellationToken = default);
		Task<Scope> UpdateScopeAsync(Scope scope, CancellationToken cancellationToken = default);
		Task<ScopeDetail?> UpdateScopeRequirementsAsync(Guid scopeId, StandardVersion standardVersion, List<Guid> idsToAdd, List<Guid> idsToRemove, bool addAll, bool removeAll, CancellationToken cancellationToken);

		Task<StandardVersion?> GetStandardVersionByIdAsync(Guid id, CancellationToken cancellationToken);
        IQueryable<StandardVersion> GetStandardVersions();
        IQueryable<StandardVersionSummary> GetStandardVersionsForGrid(StandardVersionSummaryFilters? filters);
        Task<StandardVersionDetail?> GetStandardVersionDetailByIdAsync(Guid id, CancellationToken cancellationToken);
        Task<StandardVersionChildCounts> GetChildCountsForStandardVersionAsync(Guid id, CancellationToken cancellationToken);
        Task<int?> GetStandardVersionTotalRequirementsAsync(Guid id, CancellationToken cancellationToken);

		IQueryable<StatementExport> GetStatementsForExport(StatementSummaryFilters? filters);
        IQueryable<StandardVersionExport> GetStandardVersionsForExport(StandardVersionSummaryFilters? filters);
        Task<IReadOnlyDictionary<Guid, bool>> HasOtherDraftStandardVersionsForStandardsAsync(IEnumerable<Guid> entityIds, CancellationToken cancellationToken = default);
        IQueryable<StatementSummary> GetStatementsForGrid(StatementSummaryFilters? filters);
        Task<StatementDetail?> GetStatementDetailByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<Statement?> GetStatementByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<StatementChildCounts> GetChildCountsForStatementAsync(Guid id, CancellationToken cancellationToken);

        Task<Statement> CreateStatementAsync(Statement statement, ICollection<RequirementScopes> requirementScopeCombinations, CancellationToken cancellationToken = default);
        Task<Statement> UpdateStatementAsync(Statement statement, ICollection<RequirementScopes> requirementScopeCombinations, CancellationToken cancellationToken = default);
        IQueryable<TaskSummary> GetTasksForGrid(TaskSummaryFilters? filters);
        IQueryable<TaskExport> GetTasksForExport(TaskSummaryFilters? filters);
        Task<TaskDetail?> GetTaskDetailByIdAsync(Guid id, CancellationToken cancellationToken);
        Task<Domain.Models.Task?> GetTaskByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<TaskChildCounts> GetChildCountsForTaskAsync(Guid id, CancellationToken cancellationToken);
        Task<Domain.Models.Task> CreateTaskAsync(Domain.Models.Task task, CancellationToken cancellationToken = default);
        Task<Domain.Models.Task> UpdateTaskAsync(Domain.Models.Task task, CancellationToken cancellationToken = default);
        #endregion Entity methods

        #region Lookup methods

        IQueryable<CommentType> GetCommentTypes();
        IQueryable<Standard> GetStandards();
        Task<ICollection<FilterItem>> GetStandardsLookupAsync(CancellationToken cancellationToken);
		Task<ICollection<FilterItemEntity>> GetStandardVersionsLookupAsync(CancellationToken cancellationToken);
		Task<ICollection<FilterItemEntity>> GetStandardVersionSectionsLookupAsync(Guid standardVersionId, CancellationToken cancellationToken);
		Task<ICollection<FilterItemEntity>> GetScopesLookupAsync(CancellationToken cancellationToken);
		IQueryable<TaskType> GetTaskTypes();
        Task<ICollection<FilterItem>> GetTaskTypesLookupAsync(CancellationToken cancellationToken);

        #endregion

        #region Generic (eventual Framework)

        Task<Guid?> GetWorkflowIdAsync(Guid moduleId, string entityType, CancellationToken cancellationToken);
        Guid? GetWorkflowId(Guid moduleId, string entityType);
        Task<List<Guid>> GetDefaultOwnerRoleIdsForEntityTypeAsync(Guid moduleId, string entityType, CancellationToken cancellationToken = default);

        Task<PawsState?> GetWorkflowStatusByIdAsync(Guid id, CancellationToken cancellationToken = default);

        Task<ICollection<CommentDetail>> GetCommentsForEntityAsync(Guid entityId, CancellationToken cancellationToken);
        Task<int> GetCommentsCountForEntityAsync(Guid entityId, CancellationToken cancellationToken);
        Task<Comment?> GetCommentByIdAsync(long id, CancellationToken cancellationToken);
        Task<int> GetChangeRecordsCountForEntityAsync(Guid entityId, CancellationToken cancellationToken);
        Task<ICollection<ChangeRecord>> GetChangeRecordsForEntityAsync(Guid entityId, CancellationToken cancellationToken);
		Task<ICollection<ChangeRecordItem>> GetChangeRecordItemsForChangeRecordAsync(long changeRecordId, CancellationToken cancellationToken);
        Task AddChangeRecordItem(Guid entityID, string fieldName, string friendlyFieldName, string oldValue, string newValue, CancellationToken cancellationToken);

		Task<PawsEntityDetail?> GetCurrentWorkflowStatusForEntityAsync(Guid entityId, CancellationToken cancellationToken);

        Task<IReadOnlyDictionary<Guid, string?>> GetWorkflowPseudoStateTitleForEntitiesAsync(List<Guid> entityIds, CancellationToken cancellationToken = default);

        Task<ICollection<SavedSearch>> GetSavedSearchesByContextAsync(string context, CancellationToken cancellationToken);
		IQueryable<SavedSearch> GetSavedSearchesForGridAsync(SavedSearchesGridFilters filters, CancellationToken cancellationToken);
        Task<ICollection<string>> GetSavedSearchContextsAsync(CancellationToken cancellationToken);
        Task<bool> DeleteSavedSearchAsync(SavedSearch savedSearch, CancellationToken cancellationToken);
        Task<Result> DeleteSavedSearchAsync(string name, string context, CancellationToken cancellationToken);
		Task<SavedSearch?> GetSavedSearchAsync(string context, string name, CancellationToken cancellationToken);
		Task<SavedSearch> SaveSavedSearchAsync(SavedSearch savedSearch, bool isNew, CancellationToken cancellationToken);

		#endregion Generic (eventual Framework)

		#region Other

		Task SaveStatementsAndTasks(
            ICollection<Statement> statements,
            ICollection<StatementRequirementScope> statementRequirementScopes,
            ICollection<Domain.Models.Task> tasks, 
            CancellationToken cancellationToken);

        Task<StatementRequirementScope?> GetRequirementScopeCombination(Guid requirementId, Guid scopeId, CancellationToken cancellationToken);

        Task<CommentDetail?> CreateCommentAsync(Comment comment, CancellationToken cancellationToken);

        Task<bool> DeleteCommentByIdAsync(long id, CancellationToken cancellationToken);

        Task<string> GenerateSerialNumberAsync(
            Guid moduleId,
            Guid instanceId,
            string entityType,
            Dictionary<string, object>? tokenValues = null,
            CancellationToken cancellationToken = default);

        Task<List<string>> GenerateSerialNumbersAsync(
            Guid moduleId,
            Guid instanceId,
            string entityType,
            int numberToGenerate,
            Func<int, Dictionary<string, object>?>? tokenValuesFactory = null,
            CancellationToken cancellationToken = default);

        IQueryable<UserAndPost> GetPostsWithUsers(string? searchText, ICollection<Guid> postIds, bool includeDeletedUsers = false, bool includedDeletedPosts = false);

        #endregion Other

        #region Dashboard 

        Task<DashboardInfo> CreateDashBoardInfoAsync(DashboardInfo dashboardInfo, CancellationToken cancellationToken);

        Task<DashboardInfo> UpdateDashBoardInfoAsync(DashboardInfo dashboardInfo, CancellationToken cancellationToken);

        Task<DashboardInfo> UpsertDashboardInfoAsync(DashboardInfo dashboardInfo, CancellationToken cancellationToken = default);

        Task<DashboardInfo?> GetDashboardInfoAsync(Guid id, CancellationToken cancellationToken);

        Task<ICollection<MyWorkSummaryItem>> GetMyWorkSummaryItemsAsync(
            Guid myPostID,
            IEnumerable<Guid> teamPostIDs,
            IEnumerable<Guid> groupIDs,
            string createdByOption,
            string ownedByOption,
            string progressedByOption,
            IEnumerable<Guid> roles,
            CancellationToken cancellationToken);

        Task<ICollection<MyWorkActivity>> GetMyWorkActivitiesAsync(
            Guid myPostID,
            Guid? selectedPostID,
            string entityTypeTitle,
            IEnumerable<Guid> teamPostIDs,
            IEnumerable<Guid> groupIDs,
            string createdByOption,
            string ownedByOption,
            string progressedByOption,
            IEnumerable<Guid> roles,
            CancellationToken cancellationToken);

        IQueryable<MyWorkDetailItem> GetMyWorkDetailItems(MyWorkDetailSupplementedFilters filters);

        #endregion Dashboard
    }
}
