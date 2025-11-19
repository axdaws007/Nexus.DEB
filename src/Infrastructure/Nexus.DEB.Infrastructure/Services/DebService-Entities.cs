using Mapster;
using Microsoft.EntityFrameworkCore;
using Nexus.DEB.Application.Common.Models;
using Nexus.DEB.Application.Common.Models.Filters;
using Nexus.DEB.Domain.Models;
using Nexus.DEB.Domain.Models.Common;

namespace Nexus.DEB.Infrastructure.Services
{
    public partial class DebService
    {
        #region EntityHead

        public async Task<EntityHead?> GetEntityHeadAsync(Guid id, CancellationToken cancellationToken)
        {
            return await _dbContext.EntityHeads.FirstOrDefaultAsync(x => x.EntityId == id, cancellationToken);
        }

        public async Task<EntityHeadDetail?> GetEntityHeadDetailAsync(Guid id, CancellationToken cancellationToken)
            => await _dbContext.EntityHeadDetails
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.EntityId == id, cancellationToken);
            
        #endregion

        // --------------------------------------------------------------------------------------------------------------

        #region Requirements

        public IQueryable<RequirementSummary> GetRequirementsForGrid(RequirementSummaryFilters? filters)
        {
            var query = _dbContext.Requirements
                .Include(r => r.SectionRequirements)
                    .ThenInclude(sr => sr.Section)
                .Include(r => r.StatementsRequirementsScopes)
                .Select(r => new RequirementSummary
                {
                    EntityId = r.EntityId,
                    SerialNumber = r.SerialNumber,
                    Title = r.Title,
                    LastModifiedDate = r.LastModifiedDate,
                    Sections = r.SectionRequirements
                        .Where(sr => sr.IsEnabled) // Only include enabled sections
                        .OrderBy(sr => sr.Ordinal)  // Maintain the order
                        .Select(sr => new ChildItem
                        {
                            Id = sr.Section.Id,
                            Reference = sr.Section.Reference ?? string.Empty
                        })
                        .ToList(),
                    StatusId = _dbContext.PawsStates
                        .Where(ps => ps.EntityId == r.EntityId)
                        .Select(ps => ps.StatusId)
                        .FirstOrDefault(),
                    Status = _dbContext.PawsStates
                        .Where(ps => ps.EntityId == r.EntityId)
                        .Select(ps => ps.Status)
                        .FirstOrDefault(),
                    StatementIds = r.StatementsRequirementsScopes
                        .Select(srs => srs.StatementId)
                        .Distinct()
                        .ToList()
                })
                .AsNoTracking();

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

                if (filters.StatementId.HasValue)
                {
                    query = query.Where(r => r.StatementIds.Contains(filters.StatementId.Value));
                }
            }

            return query;
        }

        public IQueryable<RequirementExport> GetRequirementsForExport(RequirementSummaryFilters? filters)
        {
            var query = _dbContext.RequirementExport.AsNoTracking();

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

            return query.OrderBy(x => x.SerialNumber);
        }

        public IQueryable<Requirement> GetRequirementsForStandardVersion(Guid standardVersionId)
        {
            var query = _dbContext.Requirements
                .Include(x => x.StandardVersions)
                .Include(x => x.Scopes)
                .Where(r => r.StandardVersions.Any(sv => sv.EntityId == standardVersionId) && r.IsRemoved == false)
                .Select(r => r);

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

        public IQueryable<ScopeExport> GetScopesForExport() => _dbContext.ScopeExport.AsNoTracking();

        #endregion Scopes

        // --------------------------------------------------------------------------------------------------------------

        #region Statements

        public IQueryable<StatementSummary> GetStatementsForGrid(StatementSummaryFilters? filters)
        {
            var query = _dbContext.Statements
                .Select(s => new StatementSummary
                {
                    EntityId = s.EntityId,
                    SerialNumber = s.SerialNumber,
                    Title = s.Title,
                    LastModifiedDate = s.LastModifiedDate,
                    OwnedById = s.OwnedById,

                    // NEW: Get Requirements through StatementRequirementScope linking table
                    Requirements = s.StatementsRequirementsScopes
                        .Select(srs => srs.Requirement)
                        .Distinct() // In case a requirement appears with multiple scopes
                        .OrderBy(r => r.SerialNumber)
                        .Select(r => new ChildItem
                        {
                            Id = r.EntityId,
                            Reference = r.SerialNumber ?? string.Empty
                        })
                        .ToList(),

                    StatusId = _dbContext.PawsStates
                        .Where(ps => ps.EntityId == s.EntityId)
                        .Select(ps => ps.StatusId)
                        .FirstOrDefault(),
                    Status = _dbContext.PawsStates
                        .Where(ps => ps.EntityId == s.EntityId)
                        .Select(ps => ps.Status)
                        .FirstOrDefault(),
                    OwnedBy = _dbContext.ViewPosts
                        .Where(p => p.Id == s.OwnedById)
                        .Select(p => p.Title)
                        .FirstOrDefault()
                })
                .AsNoTracking();

            // Apply filters
            if (filters != null)
            {
                // Filter by StandardVersion (using new linking table)
                if (filters.StandardVersionIds != null && filters.StandardVersionIds.Count > 0)
                {
                    var statementIds = _dbContext.Set<Statement>()
                        .Where(s => s.StatementsRequirementsScopes
                            .Any(srs => srs.Requirement.StandardVersions
                                .Any(sv => filters.StandardVersionIds.Contains(sv.EntityId))))
                        .Select(s => s.EntityId);

                    query = query.Where(s => statementIds.Contains(s.EntityId));
                }

                // Filter by Scope (using new linking table)
                if (filters.ScopeIds != null && filters.ScopeIds.Count > 0)
                {
                    var statementIds = _dbContext.Set<StatementRequirementScope>()
                        .Where(r => filters.ScopeIds.Contains(r.ScopeId))
                        .Select(r => r.StatementId)
                        .Distinct();

                    query = query.Where(s => statementIds.Contains(s.EntityId));
                }

                // Text search on Title
                if (!string.IsNullOrWhiteSpace(filters.SearchText))
                {
                    query = query.Where(s => s.Title.Contains(filters.SearchText));
                }

                // Date range filter
                if (filters.ModifiedFrom.HasValue)
                {
                    query = query.Where(s => s.LastModifiedDate >= filters.ModifiedFrom.Value);
                }

                if (filters.ModifiedTo.HasValue)
                {
                    query = query.Where(s => s.LastModifiedDate <= filters.ModifiedTo.Value);
                }

                if (filters.StatusIds != null && filters.StatusIds.Count > 0)
                {
                    query = query.Where(x => filters.StatusIds.Contains(x.StatusId));
                }

                if (filters.OwnedByIds != null && filters.OwnedByIds.Count > 0)
                {
                    query = query.Where(x => filters.OwnedByIds.Contains(x.OwnedById));
                }
            }

            return query;
        }

        public IQueryable<StatementExport> GetStatementsForExport(StatementSummaryFilters? filters)
        {
            var query = _dbContext.StatementExport.AsNoTracking();

            if (filters != null)
            {
                // Filter by StandardVersion (using navigation property)
                if (filters.StandardVersionIds != null && filters.StandardVersionIds.Count > 0)
                {
                    var statementIds = _dbContext.Set<StatementRequirementScope>()
                        .Where(s => s.Requirement.StandardVersions.Any(sv => filters.StandardVersionIds.Contains(sv.EntityId)))
                        .Select(s => s.StatementId);

                    query = query.Where(s => statementIds.Contains(s.EntityId));
                }

                // Filter by Scope (using navigation property)
                if (filters.ScopeIds != null && filters.ScopeIds.Count > 0)
                {
                    var statementIds = _dbContext.Set<StatementRequirementScope>()
                        .Where(r => filters.ScopeIds.Contains(r.ScopeId))
                        .Select(r => r.StatementId)
                        .Distinct();

                    query = query.Where(r => statementIds.Contains(r.EntityId));
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

                if (filters.OwnedByIds != null && filters.OwnedByIds.Count > 0)
                {
                    query = query.Where(x => filters.OwnedByIds.Contains(x.OwnedById));
                }
            }

            return query.OrderBy(x => x.SerialNumber);
        }

        public async Task<StatementDetail?> GetStatementByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var statement = await _dbContext.StatementDetails.FirstOrDefaultAsync(x => x.EntityId == id, cancellationToken);

            if (statement == null)
                return null;

            var statementDetail = statement.Adapt<StatementDetail>();

            statementDetail.RequirementScopeCombinations = await _dbContext.StatementsRequirementsScopes
                                                                    .Include(x => x.Requirement)
                                                                    .Include(x => x.Scope)
                                                                    .Where(x => x.StatementId == id)
                                                                    .Select(x => new RequirementScopeDetail()
                                                                    {
                                                                        RequirementId = x.RequirementId,
                                                                        RequirementSerialNumber = x.Requirement.SerialNumber,
                                                                        RequirementTitle = x.Requirement.Title,
                                                                        ScopeId = x.ScopeId,
                                                                        ScopeSerialNumber = x.Scope.SerialNumber,
                                                                        ScopeTitle = x.Scope.Title
                                                                    }).ToListAsync(cancellationToken);

            return statementDetail;
        }

            
        public async Task<Statement> SaveStatementAsync(
            Statement statement,
            CancellationToken cancellationToken = default)
        {
            await _dbContext.Statements.AddAsync(statement, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return statement;
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

        public IQueryable<StandardVersionSummary> GetStandardVersionsForGrid(StandardVersionSummaryFilters? filters)
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

        public IQueryable<StandardVersionExport> GetStandardVersionsForExport(StandardVersionSummaryFilters? filters)
        {
            var query = _dbContext.StandardVersionExport.AsNoTracking(); 

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
                    query = query.Where(r => r.EffectiveStartDate >= filters.EffectiveFromDate.Value);
                }

                if (filters.EffectiveToDate.HasValue)
                {
                    query = query.Where(r => r.EffectiveEndDate < filters.EffectiveToDate.Value.AddDays(1));
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
                        .Where(t => t.Statement.StatementsRequirementsScopes
                            .Any(srs => srs.Requirement.StandardVersions
                                .Any(sv => filters.StandardVersionIds.Contains(sv.EntityId))))
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

                if (filters.OwnedByIds != null && filters.OwnedByIds.Count > 0)
                {
                    query = query.Where(x => filters.OwnedByIds.Contains(x.OwnedById));
                }
            }

            return query;
        }

        public IQueryable<TaskExport> GetTasksForExport(TaskSummaryFilters? filters)
        {
            var query = _dbContext.TaskExport.AsNoTracking();

            if (filters != null)
            {
                // Filter by StandardVersion (using navigation property)
                if (filters.StandardVersionIds != null && filters.StandardVersionIds.Count > 0)
                {
                    var taskIds = _dbContext.Set<Domain.Models.Task>()
                        .Where(t => t.Statement.StatementsRequirementsScopes
                            .Any(srs => srs.Requirement.StandardVersions
                                .Any(sv => filters.StandardVersionIds.Contains(sv.EntityId))))
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

                if (filters.OwnedByIds != null && filters.OwnedByIds.Count > 0)
                {
                    query = query.Where(x => filters.OwnedByIds.Contains(x.OwnedById));
                }
            }

            return query.OrderBy(x => x.SerialNumber);
        }

        #endregion Tasks

        // --------------------------------------------------------------------------------------------------------------

        #region Other

        public async System.Threading.Tasks.Task SaveStatementsAndTasks(
            ICollection<Statement> statements,
            ICollection<StatementRequirementScope> statementRequirmementScopes,
            ICollection<Domain.Models.Task> tasks, 
            CancellationToken cancellationToken)
        {
            await _dbContext.Statements.AddRangeAsync(statements, cancellationToken);
            await _dbContext.StatementsRequirementsScopes.AddRangeAsync(statementRequirmementScopes, cancellationToken);
            await _dbContext.Tasks.AddRangeAsync(tasks, cancellationToken);

            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        #endregion
    }
}
