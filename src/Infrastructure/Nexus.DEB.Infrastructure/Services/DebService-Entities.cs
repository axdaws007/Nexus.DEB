using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Nexus.DEB.Application.Common.Models;
using Nexus.DEB.Application.Common.Models.Filters;
using Nexus.DEB.Domain.Models;
using Nexus.DEB.Domain.Models.Common;
using System.Linq;

namespace Nexus.DEB.Infrastructure.Services
{
    public partial class DebService
    {
        #region EntityHead

        public async Task<EntityHead?> GetEntityHeadAsync(Guid id, CancellationToken cancellationToken)
        {
            return await _dbContext.EntityHeads.FirstOrDefaultAsync(x => x.EntityId == id, cancellationToken);
        }

        #endregion

        #region Requirements

        public IQueryable<RequirementSummary> GetRequirementsForGrid(RequirementSummaryFilters? filters)
        {
            var query = _dbContext.RequirementSummaries.AsQueryable();

            if (filters != null)
            {
                // Filter by StandardVersion (using navigation property)
                if (filters.StandardVersionIds != null && filters.StandardVersionIds.Count > 0)
                {
                    var requirementIds = _dbContext.Set<Requirement>()
                        .Where(r => r.StandardVersions.Any(sv => filters.StandardVersionIds.Contains(sv.EntityId)))
                        .Select(r => r.EntityId);

                    query = query.Where(r => requirementIds.Contains(r.EntityId));
                }

                // Filter by Scope (using navigation property)
                if (filters.ScopeIds != null && filters.ScopeIds.Count > 0)
                {
                    var requirementIds = _dbContext.Set<Requirement>()
                        .Where(r => r.Scopes.Any(s => filters.ScopeIds.Contains(s.EntityId)))
                        .Select(r => r.EntityId);

                    query = query.Where(r => requirementIds.Contains(r.EntityId));
                }

                if (filters.StatusIds != null && filters.StatusIds.Count > 0)
                {
                    query = query.Where(x => filters.StatusIds.Contains(x.StatusId));
                }

                // Text search on Title
                if (!string.IsNullOrWhiteSpace(filters.SearchText))
                {
                    query = query.Where(r => r.Title.Contains(filters.SearchText));
                }

                // Date range filter
                if (filters.ModifiedFrom.HasValue)
                {
                    query = query.Where(r => r.LastModifiedDate >= filters.ModifiedFrom.Value);
                }

                if (filters.ModifiedTo.HasValue)
                {
                    query = query.Where(r => r.LastModifiedDate <= filters.ModifiedTo.Value);
                }
            }

            return query;
        }

        #endregion Requirements

        // --------------------------------------------------------------------------------------------------------------

        #region Scopes

        public IQueryable<Scope> GetScopes()
        {
            var query = from s in _dbContext.Scopes
                        where s.IsRemoved == false
                        select s;

            return query;
        }

        public async Task<ICollection<FilterItemEntity>> GetScopesLookupAsync(CancellationToken cancellationToken)
        {
            return await (from sc in _dbContext.Scopes
                          orderby sc.Title
                          select new FilterItemEntity()
                          {
                              Id = sc.EntityId,
                              Value = sc.Title,
                              IsEnabled = !sc.IsRemoved
                          }).ToListAsync(cancellationToken);
        }


        public IQueryable<ScopeSummary> GetScopesForGrid() => _dbContext.ScopeSummaries.AsNoTracking();

        #endregion Scopes

        // --------------------------------------------------------------------------------------------------------------

        #region Statements

        public IQueryable<StatementSummary> GetStatementsForGrid(StatementSummaryFilters? filters)
        {
            var query = _dbContext.StatementSummaries.AsQueryable();

            if (filters != null)
            {
                // Filter by StandardVersion (using navigation property)
                if (filters.StandardVersionIds != null && filters.StandardVersionIds.Count > 0)
                {
                    var statementIds = _dbContext.Set<Statement>()
                        .Where(s => s.Requirements.Any(r => r.StandardVersions.Any(sv => filters.StandardVersionIds.Contains(sv.EntityId))))
                        .Select(s => s.EntityId);

                    query = query.Where(s => statementIds.Contains(s.EntityId));
                }

                // Filter by Scope (using navigation property)
                if (filters.ScopeIds != null && filters.ScopeIds.Count > 0)
                {
                    var requirementIds = _dbContext.Set<Requirement>()
                        .Where(r => r.Scopes.Any(s => filters.ScopeIds.Contains(s.EntityId)))
                        .Select(r => r.EntityId);

                    query = query.Where(r => requirementIds.Contains(r.EntityId));
                }

                // Text search on Title
                if (!string.IsNullOrWhiteSpace(filters.SearchText))
                {
                    query = query.Where(r => r.Title.Contains(filters.SearchText));
                }

                // Date range filter
                if (filters.ModifiedFrom.HasValue)
                {
                    query = query.Where(r => r.LastModifiedDate >= filters.ModifiedFrom.Value);
                }

                if (filters.ModifiedTo.HasValue)
                {
                    query = query.Where(r => r.LastModifiedDate <= filters.ModifiedTo.Value);
                }

                if (filters.StatusIds != null && filters.StatusIds.Count > 0)
                {
                    query = query.Where(x => filters.StatusIds.Contains(x.StatusId));
                }

                // TODO
                // OwnedById filter
            }

            return query;
        }

        #endregion Statements

        // --------------------------------------------------------------------------------------------------------------

        #region StandardVersions

        public async Task<StandardVersion?> GetStandardVersionByIdAsync(Guid id, CancellationToken cancellationToken)
            => await _dbContext.StandardVersions.FirstOrDefaultAsync(x => x.EntityId == id, cancellationToken);

        public IQueryable<StandardVersion> GetStandardVersions()
        {
            var query = from sv in _dbContext.StandardVersions
                                        .Include(x => x.Standard)
                        where sv.IsRemoved == false
                        select sv;

            return query;
        }

        public async Task<ICollection<FilterItemEntity>> GetStandardVersionsLookupAsync(CancellationToken cancellationToken)
        {
            var results = await (from sv in _dbContext.StandardVersions
                                               .Include(x => x.Standard)
                                  select new FilterItemEntity()
                                  {
                                      Id = sv.EntityId,
                                      Value = sv.Standard.Title + " " + sv.Reference,
                                      IsEnabled = !sv.IsRemoved
                                  }).ToListAsync(cancellationToken);

            return results.OrderBy(x => x.Value).ToList();
        }

        public IQueryable<StandardVersionSummary> GetStandardVersionsForExportOrGrid(StandardVersionSummaryFilters? filters)
        {
            var query = _dbContext.StandardVersionSummaries.AsNoTracking();

            if (filters != null)
            {
                if (filters.StandardIds != null && filters.StandardIds.Count > 0)
                {
                    query = query.Where(x => filters.StandardIds.Contains(x.StandardId));
                }

                if (filters.StatusIds != null && filters.StatusIds.Count > 0)
                {
                    query = query.Where(x => filters.StatusIds.Contains(x.StatusId));
                }

                if (filters.EffectiveFromDate.HasValue)
                {
                    query = query.Where(r => r.EffectiveFrom >= filters.EffectiveFromDate.Value);
                }

                if (filters.EffectiveToDate.HasValue)
                {
                    query = query.Where(r => r.EffectiveTo < filters.EffectiveToDate.Value.AddDays(1));
                }
            }

            return query;
        }


        #endregion StandardVersions

        // --------------------------------------------------------------------------------------------------------------

        #region Tasks

        public IQueryable<TaskSummary> GetTasksForGrid(TaskSummaryFilters? filters)
        {
            var query = _dbContext.TaskSummaries.AsQueryable();

            if (filters != null)
            {
                // Filter by StandardVersion (using navigation property)
                if (filters.StandardVersionIds != null && filters.StandardVersionIds.Count > 0)
                {
                    var taskIds = _dbContext.Set<Domain.Models.Task>()
                        .Where(t => t.Statement.Requirements.Any(r => r.StandardVersions.Any(sv => filters.StandardVersionIds.Contains(sv.EntityId))))
                        .Select(t => t.EntityId);

                    query = query.Where(t => taskIds.Contains(t.EntityId));
                }

                // Text search on Title
                if (!string.IsNullOrWhiteSpace(filters.SearchText))
                {
                    query = query.Where(r => r.Title.Contains(filters.SearchText));
                }

                // Date range filter
                if (filters.DueDateFrom.HasValue)
                {
                    query = query.Where(r => r.DueDate >= filters.DueDateFrom.Value);
                }

                if (filters.DueDateTo.HasValue)
                {
                    query = query.Where(r => r.DueDate <= filters.DueDateTo.Value);
                }

                if (filters.StatusIds != null && filters.StatusIds.Count > 0)
                {
                    query = query.Where(x => filters.StatusIds.Contains(x.StatusId));
                }

                if (filters.StatementId.HasValue)
                {
                    query = query.Where(x => x.StatementId == filters.StatementId.Value);
                }

                // TODO
                // OwnedById filter
            }

            return query;

        }
        #endregion Tasks
    }
}
