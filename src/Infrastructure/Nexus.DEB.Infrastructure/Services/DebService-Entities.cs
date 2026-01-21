using Mapster;
using Microsoft.EntityFrameworkCore;
using Nexus.DEB.Application.Common.Models;
using Nexus.DEB.Application.Common.Models.Filters;
using Nexus.DEB.Domain;
using Nexus.DEB.Domain.Models;
using Nexus.DEB.Domain.Models.Common;
using System.Collections.Generic;

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

        public async Task<Dictionary<Guid, EntityHead>> GetEntityHeadsAsync(
            IEnumerable<Guid> ids,
            CancellationToken cancellationToken)
        {
            var idList = ids.ToList();

            return await _dbContext.EntityHeads
                .AsNoTracking()
                .Where(e => idList.Contains(e.EntityId))
                .ToDictionaryAsync(e => e.EntityId, cancellationToken);
        }

        #endregion

        // --------------------------------------------------------------------------------------------------------------

        #region Requirements

        public IQueryable<RequirementSummary> GetRequirementsForGrid(RequirementSummaryFilters? filters)
        {
            var query = _dbContext.Requirements
                .Include(r => r.SectionRequirements)
                    .ThenInclude(sr => sr.Section)
                .Include(r => r.StatementsRequirementsScopes)
                .Include(r => r.StandardVersions)
                    .ThenInclude(x => x.Standard)
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
                        .ToList(),
                    StandardVersionTitles = r.StandardVersions.Select(sv => sv.Title).ToList()
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
                    var from = filters.ModifiedFrom.Value.ToDateTime(TimeOnly.MinValue);
                    query = query.Where(r => r.LastModifiedDate >= from);
                }

                if (filters.ModifiedTo.HasValue)
                {
                    var to = filters.ModifiedTo.Value.AddDays(1).ToDateTime(TimeOnly.MinValue);
                    query = query.Where(r => r.LastModifiedDate < to);
                }

                if (filters.StatementId.HasValue)
                {
                    query = query.Where(r => r.StatementIds.Contains(filters.StatementId.Value));
                }

                if (filters.OnlyShowAvailableRequirementScopeCombinations && filters.ScopeIds != null && filters.ScopeIds.Count > 0)
                {
                    var requirementIdsWithAvailableCombinations = _dbContext.Set<Requirement>()
                        .Where(r => r.Scopes.Any(s =>
                            // Scope is in our filter list
                            filters.ScopeIds.Contains(s.EntityId) &&
                            // AND this requirement/scope combination doesn't exist in allocations
                            // (r.StatementsRequirementsScopes is already scoped to THIS requirement)
                            !r.StatementsRequirementsScopes.Any(srs => srs.ScopeId == s.EntityId)))
                        .Select(r => r.EntityId);

                    query = query.Where(r => requirementIdsWithAvailableCombinations.Contains(r.EntityId));
                }
            }

            return query;
        }

        public IQueryable<StandardVersionRequirementDetail> GetStandardVersionRequirementsForGrid(StandardVersionRequirementsFilters? filters)
		{
            var query = from svr in _dbContext.StandardVersionRequirements
                        join r in (_dbContext.Requirements.Include(r => r.Scopes)) on svr.RequirementId equals r.EntityId
                        select new StandardVersionRequirementDetail
                        {
							RequirementId = svr.RequirementId,
                            SerialNumber = svr.SerialNumber,
                            Title = svr.Title,
                            StandardVersionId = svr.StandardVersionId,
                            StandardVersion = svr.StandardVersion,
                            SectionId = svr.SectionId,
                            Section = svr.Section,
                            OtherScopes = r.Scopes.Where(w => filters == null || filters.ScopeId == null || w.EntityId != filters.ScopeId).Count()
                        };

            if(filters != null)
            {
                if(filters.StandardVersionId.HasValue)
				{
					query = query.Where(w => w.StandardVersionId == filters.StandardVersionId.Value);
				}

                if(filters.SectionId.HasValue)
                {
					query = query.Where(w => w.SectionId == filters.SectionId.Value);
				}

                if(filters.SearchText != null && !string.IsNullOrWhiteSpace(filters.SearchText))
                {
					query = query.Where(w => w.Title.Contains(filters.SearchText) || w.SerialNumber.Contains(filters.SearchText));
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
                    var from = filters.ModifiedFrom.Value.ToDateTime(TimeOnly.MinValue);
                    query = query.Where(r => r.LastModifiedDate >= from);
                }

                if (filters.ModifiedTo.HasValue)
                {
                    var to = filters.ModifiedTo.Value.AddDays(1).ToDateTime(TimeOnly.MinValue);
                    query = query.Where(r => r.LastModifiedDate < to);
                }
            }

            return query.OrderBy(x => x.SerialNumber);
        }

		public async Task<RequirementDetail?> GetRequirementByIdAsync(Guid id, CancellationToken cancellationToken)
		{
			var requirement = await _dbContext.Requirements.AsNoTracking()
				.Include(r => r.RequirementType)
				.Include(r => r.RequirementCategory)
				.FirstOrDefaultAsync(s => s.EntityId == id, cancellationToken);

			if (requirement == null)
				return null;

			var requirementDetail = requirement.Adapt<RequirementDetail>();

            requirementDetail.RequirementTypeTitle = requirement.RequirementType?.Title;
            requirementDetail.RequirementCategoryTitle = requirement.RequirementCategory?.Title;

			return requirementDetail;
		}

		public async Task<RequirementChildCounts> GetChildCountsForRequirementAsync(Guid id, CancellationToken cancellationToken)
		{
			var numberOfComments = await GetCommentsCountForEntityAsync(id, cancellationToken);
			var numberOfHistoryEvents = await GetChangeRecordsCountForEntityAsync(id, cancellationToken);

			return new RequirementChildCounts()
			{
				CommentsCount = numberOfComments,
				HistoryCount = numberOfHistoryEvents
			};
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

        public async Task<ICollection<RequirementWithScopes>> GetRequirementScopesForStatement(
            Guid statementId,
            CancellationToken cancellationToken)
        {
            var results = await _dbContext.Set<StatementRequirementScope>()
                .Where(srs => srs.StatementId == statementId)
                .Include(srs => srs.Requirement)
                    .ThenInclude(r => r.StandardVersions)
                        .ThenInclude(s => s.Standard)
                .Include(srs => srs.Scope)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            var grouped = results
                .GroupBy(srs => new
                {
                    srs.RequirementId,
                    srs.Requirement.EntityId,
                    srs.Requirement.SerialNumber,
                    srs.Requirement.Title
                })
                .Select(g => new RequirementWithScopes
                {
                    RequirementId = g.Key.EntityId,
                    SerialNumber = g.Key.SerialNumber,
                    Title = g.Key.Title,
                    StandardVersionReference = g.First().Requirement.StandardVersions
                        .Select(sv => sv.Title)
                        .FirstOrDefault() ?? string.Empty,
                    Scopes = g.Select(srs => new ScopeCondensed
                    {
                        EntityId = srs.ScopeId,
                        SerialNumber = srs.Scope.SerialNumber,
                        Title = srs.Scope.Title
                    }).ToList()
                })
                .ToList();

            return grouped;
        }

        public async Task<List<StatementRequirementScope>> GetRequirementScopeCombinations(
            IEnumerable<(Guid RequirementId, Guid ScopeId)> combinations,
            CancellationToken cancellationToken)
        {
            var combinationsList = combinations.ToList();

            // Get distinct IDs to minimize SQL results
            var requirementIds = combinationsList.Select(c => c.RequirementId).Distinct().ToList();
            var scopeIds = combinationsList.Select(c => c.ScopeId).Distinct().ToList();

            // Query with two Contains (translates to SQL IN clauses)
            var candidates = await _dbContext.StatementsRequirementsScopes
                .AsNoTracking()
                .Where(srs => requirementIds.Contains(srs.RequirementId)
                           && scopeIds.Contains(srs.ScopeId))
                .ToListAsync(cancellationToken);

            // Filter to exact combinations in memory
            var combinationSet = combinationsList.ToHashSet();
            return candidates
                .Where(srs => combinationSet.Contains((srs.RequirementId, srs.ScopeId)))
                .ToList();
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

        public async Task<ScopeDetail?> GetScopeByIdAsync(Guid id, CancellationToken cancellationToken)
		{
            var scope = await _dbContext.Scopes.AsNoTracking()
                .Include(s => s.Requirements)
                .ThenInclude(r => r.StandardVersions)
				.FirstOrDefaultAsync(s => s.EntityId == id, cancellationToken);

			if (scope == null)
				return null;

			var scopeDetail = scope.Adapt<ScopeDetail>();

            var scopeRequirements = scope.Requirements;
            var standardVersionIds = scopeRequirements.SelectMany(s => s.StandardVersions).Distinct().Select(s => s.EntityId);


			var standardVersions = _dbContext.StandardVersions.AsNoTracking().Include(sv => sv.Standard).Include(sv => sv.Requirements).Where(w => standardVersionIds.Contains(w.EntityId));
            var standardVersionStates = _dbContext.PawsEntityDetails.AsNoTracking().Where(w => standardVersionIds.Contains(w.EntityId));


			foreach (var sv in standardVersions)
            {
                var svR = new StandardVersionRequirements();
                svR.StandardVersionId = sv.EntityId;
				svR.StandardVersionTitle = sv.Title;
                svR.Status = standardVersionStates.FirstOrDefault(s => s.EntityId == sv.EntityId)?.PseudoStateTitle ?? string.Empty;
                svR.TotalRequirements = sv.Requirements.Count;
                svR.TotalRequirementsInScope = scopeRequirements.Where(w => w.StandardVersions.Any(a => a.EntityId == sv.EntityId)).Count();
                scopeDetail.StandardVersionRequirements.Add(svR);
			}

            return scopeDetail;
		}

		public async Task<ScopeChildCounts> GetChildCountsForScopeAsync(Guid id, CancellationToken cancellationToken)
		{
			var numberOfComments = await GetCommentsCountForEntityAsync(id, cancellationToken);

			var numberOfHistoryEvents = await GetChangeRecordsCountForEntityAsync(id, cancellationToken);

			// Note: AttachmentsCount populated in the GraphQL query as we're interrogating the DMS Web API.

			return new ScopeChildCounts()
			{
				CommentsCount = numberOfComments,
				HistoryCount = numberOfHistoryEvents
			};
		}

		public async Task<ICollection<ScopeCondensed>> GetScopesForRequirementAsync(
            Guid requirementId,
            Guid? statementId,
            CancellationToken cancellationToken)
        {
            var query = _dbContext.Requirements
                .Where(r => r.EntityId == requirementId)
                .SelectMany(r => r.Scopes);

            if (statementId == null)
            {
                // Exclude scopes that are used in ANY statement for this requirement
                query = query.Where(s => !_dbContext.StatementsRequirementsScopes.Any(srs =>
                    srs.RequirementId == requirementId &&
                    srs.ScopeId == s.EntityId));
            }
            else
            {
                // Exclude scopes that are used in OTHER statements for this requirement
                // (but include scopes used for THIS statement)
                query = query.Where(s => !_dbContext.StatementsRequirementsScopes.Any(srs =>
                    srs.RequirementId == requirementId &&
                    srs.ScopeId == s.EntityId &&
                    srs.StatementId != statementId.Value));
            }

            return await query
                .Select(s => new ScopeCondensed()
                {
					EntityId = s.EntityId,
                    SerialNumber = s.SerialNumber,
                    Title = s.Title
                })
                .ToListAsync(cancellationToken);
        }

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
                    EntityTypeTitle = s.EntityTypeTitle,

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
                    var from = filters.ModifiedFrom.Value.ToDateTime(TimeOnly.MinValue);
                    query = query.Where(s => s.LastModifiedDate >= from);
                }

                if (filters.ModifiedTo.HasValue)
                {
                    var to = filters.ModifiedTo.Value.AddDays(1).ToDateTime(TimeOnly.MinValue);
                    query = query.Where(s => s.LastModifiedDate < to);
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
                    var from = filters.ModifiedFrom.Value.ToDateTime(TimeOnly.MinValue);
                    query = query.Where(s => s.LastModifiedDate >= from);
                }

                if (filters.ModifiedTo.HasValue)
                {
                    var to = filters.ModifiedTo.Value.AddDays(1).ToDateTime(TimeOnly.MinValue);
                    query = query.Where(s => s.LastModifiedDate < to);
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

        public async Task<StatementDetail?> GetStatementDetailByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var statement = await _dbContext.StatementDetails.FirstOrDefaultAsync(x => x.EntityId == id, cancellationToken);

            if (statement == null)
                return null;

            var statementDetail = statement.Adapt<StatementDetail>();

            // Get the flat list of combinations first
            var combinations = await _dbContext.StatementsRequirementsScopes
                .Include(x => x.Requirement)
                    .ThenInclude(r => r.StandardVersions)
                        .ThenInclude(sv => sv.Standard)
                .Include(x => x.Scope)
                .Where(x => x.StatementId == id)
                .ToListAsync(cancellationToken);

            statementDetail.Requirements = combinations
                .GroupBy(x => x.Requirement)
                .Select(g =>
                {
                    var requirement = g.Key;
                    var standardVersion = requirement.StandardVersions.First(); // Assume only one

                    return new RequirementWithScopes
                    {
                        RequirementId = requirement.EntityId,
                        SerialNumber = requirement.SerialNumber,
                        Title = requirement.Title,

                        // StandardVersion info (single values)
                        StandardVersionReference = standardVersion.Title,

                        // All scopes for this requirement (from this statement)
                        Scopes = g.Select(x => new ScopeCondensed
                        {
							EntityId = x.ScopeId,
                            SerialNumber = x.Scope.SerialNumber,
                            Title = x.Scope.Title
                        }).ToList()
                    };
                })
                .ToList();

            return statementDetail;
        }


        public async Task<Statement?> GetStatementByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => await _dbContext.Statements.FirstOrDefaultAsync(x => x.EntityId == id, cancellationToken);

        public async Task<StatementChildCounts> GetChildCountsForStatementAsync(Guid id, CancellationToken cancellationToken)
        {
            var numberOfTasks = await _dbContext.Tasks.CountAsync(x => x.StatementId == id && x.IsRemoved == false, cancellationToken);

            var numberOfComments = await GetCommentsCountForEntityAsync(id, cancellationToken);

            var numberOfHistoryEvents = await GetChangeRecordsCountForEntityAsync(id, cancellationToken);

            // Note: EvidencesCount populated in the GraphQL query as we're interrogating the DMS Web API.

            return new StatementChildCounts()
            {
                TasksCount = numberOfTasks,
                CommentsCount = numberOfComments,
                HistoryCount = numberOfHistoryEvents
            };
        }    

        public async Task<Statement> CreateStatementAsync(
            Statement statement,
            ICollection<RequirementScopes> requirementScopeCombinations,
            CancellationToken cancellationToken = default)
        {
            await _dbContext.Statements.AddAsync(statement, cancellationToken);

            var newStatementRequirementScopes = requirementScopeCombinations
                .SelectMany(rs => rs.ScopeIds.Select(scopeId => new StatementRequirementScope
                {
                    StatementId = statement.EntityId,
                    RequirementId = rs.RequirementId,
                    ScopeId = scopeId
                }))
                .ToList();

            await _dbContext.StatementsRequirementsScopes.AddRangeAsync(newStatementRequirementScopes, cancellationToken);

            await _dbContext.SaveChangesAsync(cancellationToken);

            return statement;
        }

        public async Task<Statement> UpdateStatementAsync(
            Statement statement,
            ICollection<RequirementScopes> requirementScopeCombinations,
            CancellationToken cancellationToken = default)
        {
            var existingStatementRequirementScopes = _dbContext.StatementsRequirementsScopes.Where(x => x.StatementId == statement.EntityId).ToList();

            var newStatementRequirementScopes = requirementScopeCombinations
                .SelectMany(rs => rs.ScopeIds.Select(scopeId => new StatementRequirementScope
                {
                    StatementId = statement.EntityId,  
                    RequirementId = rs.RequirementId,
                    ScopeId = scopeId
                }))
                .ToList();


            foreach (var existingSRS in existingStatementRequirementScopes)
            {
                if (!newStatementRequirementScopes.Contains(existingSRS))
                {
                    _dbContext.StatementsRequirementsScopes.Remove(existingSRS);
                }
            }

            foreach (var newSRS in newStatementRequirementScopes)
            {
                if (!existingStatementRequirementScopes.Contains(newSRS))
                {
                    _dbContext.StatementsRequirementsScopes.Add(newSRS);
                }
            }

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
                                      Value = sv.Title,
                                      IsEnabled = !sv.IsRemoved
                                  }).ToListAsync(cancellationToken);

            return results.OrderBy(x => x.Value).ToList();
        }

		public async Task<ICollection<FilterItemEntity>> GetStandardVersionSectionsLookupAsync(Guid standardVersionId, CancellationToken cancellationToken)
		{
            var returnList = new List<FilterItemEntity>();
            try
            {
                var results = await _dbContext.Sections.Where(w => w.StandardVersionId == standardVersionId)
                                    .OrderBy(o => o.Ordinal)
                                    .ThenBy(t => t.IsReferenceDisplayed ? t.Reference : null)
									.ThenBy(t => t.IsTitleDisplayed ? t.Title : null)
									.Select(s => new FilterItemEntity()
                                    {
                                        Id = s.Id,
                                        Value = string.Format("{0} {1}", (s.IsReferenceDisplayed ? s.Reference : string.Empty), (s.IsTitleDisplayed ? s.Title : string.Empty)).Trim(),
                                        IsEnabled = true
                                    })
                                    .ToListAsync(cancellationToken);
                returnList.AddRange(results);
            }
            catch(Exception ex)
            {
                // Log and swallow exception to avoid breaking the calling query
                Console.WriteLine(ex.Message);
			}
			return returnList;
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
                    var from = filters.EffectiveFromDate.Value.ToDateTime(TimeOnly.MinValue);
                    query = query.Where(r => r.EffectiveFrom >= from);
                }

                if (filters.EffectiveToDate.HasValue)
                {
                    var to = filters.EffectiveToDate.Value.AddDays(1).ToDateTime(TimeOnly.MinValue);
                    query = query.Where(r => r.EffectiveTo < to);
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
                    var from = filters.EffectiveFromDate.Value.ToDateTime(TimeOnly.MinValue);
                    query = query.Where(r => r.EffectiveStartDate >= from);
                }

                if (filters.EffectiveToDate.HasValue)
                {
                    var to = filters.EffectiveToDate.Value.AddDays(1).ToDateTime(TimeOnly.MinValue);
                    query = query.Where(r => r.EffectiveEndDate < to);
                }
            }

            return query;
        }

        public async Task<StandardVersionDetail?> GetStandardVersionDetailByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
			var standardVersion = await _dbContext.StandardVersionDetails.FirstOrDefaultAsync(x => x.EntityId == id, cancellationToken);

			if (standardVersion == null)
				return null;

			var standardVersionDetail = standardVersion.Adapt<StandardVersionDetail>();

            var scopes = await _dbContext.StandardVersions.Where(sv => sv.EntityId == id)
				.SelectMany(sv => sv.Requirements)
				.Include(r => r.Scopes)
                .SelectMany(s => s.Scopes).Distinct()
				.ToListAsync(cancellationToken);

            standardVersionDetail.Scopes = scopes;

			return standardVersionDetail;
		}

		public async Task<StandardVersionChildCounts> GetChildCountsForStandardVersionAsync(Guid id, CancellationToken cancellationToken)
		{
			var numberOfComments = await GetCommentsCountForEntityAsync(id, cancellationToken);

			var numberOfHistoryEvents = await GetChangeRecordsCountForEntityAsync(id, cancellationToken);

			return new StandardVersionChildCounts()
			{
				CommentsCount = numberOfComments,
				HistoryCount = numberOfHistoryEvents
			};
		}

        public async Task<IReadOnlyDictionary<Guid, bool>> HasOtherDraftStandardVersionsForStandardsAsync(
            IEnumerable<Guid> entityIds,
            CancellationToken cancellationToken = default)
        {
            var entityIdList = entityIds.ToList();

            if (entityIdList.Count == 0)
                return new Dictionary<Guid, bool>();

            var query = from current in _dbContext.StandardVersions
                        where entityIdList.Contains(current.EntityId)
                        select new
                        {
                            current.EntityId,
                            HasOtherActive = (
                                from other in _dbContext.StandardVersions
                                where other.StandardId == current.StandardId
                                   && other.EntityId != current.EntityId
                                   && !other.IsRemoved
                                join ped in _dbContext.PawsEntityDetails on other.EntityId equals ped.EntityId
                                where ped.PseudoStateTitle == DebHelper.Paws.States.Draft
                                select other
                            ).Any()
                        };

            return await query.ToDictionaryAsync(
                x => x.EntityId,
                x => x.HasOtherActive,
                cancellationToken);
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
                    var from = filters.DueDateFrom.Value.ToDateTime(TimeOnly.MinValue);
                    query = query.Where(r => r.DueDate >= from);
                }

                if (filters.DueDateTo.HasValue)
                {
                    var to = filters.DueDateTo.Value.AddDays(1).ToDateTime(TimeOnly.MinValue);
                    query = query.Where(r => r.DueDate < to);
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

                if (filters.TaskTypeIds != null && filters.TaskTypeIds.Count > 0)
                {
                    query = query.Where(x => filters.TaskTypeIds.Contains(x.TaskTypeId));
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
                    var from = filters.DueDateFrom.Value.ToDateTime(TimeOnly.MinValue);
                    query = query.Where(r => r.DueDate >= from);
                }

                if (filters.DueDateTo.HasValue)
                {
                    var to = filters.DueDateTo.Value.AddDays(1).ToDateTime(TimeOnly.MinValue);
                    query = query.Where(r => r.DueDate < to);
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

                if (filters.TaskTypeIds != null && filters.TaskTypeIds.Count > 0)
                {
                    query = query.Where(x => filters.TaskTypeIds.Contains(x.TaskTypeId));
                }
            }

            return query.OrderBy(x => x.SerialNumber);
        }

		public async Task<TaskDetail?> GetTaskDetailByIdAsync(Guid id, CancellationToken cancellationToken = default)
		{
			var task = await _dbContext.TaskDetails.FirstOrDefaultAsync(x => x.EntityId == id, cancellationToken);

			if (task == null)
				return null;

			var taskDetail = task.Adapt<TaskDetail>();

			return taskDetail;
		}

        public async Task<Domain.Models.Task?> GetTaskByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => await _dbContext.Tasks.FirstOrDefaultAsync(x => x.EntityId == id, cancellationToken);

        public async Task<TaskChildCounts> GetChildCountsForTaskAsync(Guid id, CancellationToken cancellationToken)
        {
            var numberOfComments = await GetCommentsCountForEntityAsync(id, cancellationToken);

            var numberOfHistoryEvents = await GetChangeRecordsCountForEntityAsync(id, cancellationToken);

            return new TaskChildCounts()
            {
                CommentsCount = numberOfComments,
                HistoryCount = numberOfHistoryEvents
            };
        }

        public async Task<Domain.Models.Task> CreateTaskAsync(Domain.Models.Task task, CancellationToken cancellationToken = default)
        {
            await _dbContext.Tasks.AddAsync(task, cancellationToken);

            await _dbContext.SaveChangesAsync(cancellationToken);

            return task;
        }

        public async Task<Domain.Models.Task> UpdateTaskAsync(Domain.Models.Task task, CancellationToken cancellationToken = default)
        {
            await _dbContext.SaveChangesAsync(cancellationToken);

            return task;
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

        public async Task<StatementRequirementScope?> GetRequirementScopeCombination(Guid requirementId, Guid scopeId, CancellationToken cancellationToken)
            => await _dbContext.StatementsRequirementsScopes
                    .FirstOrDefaultAsync(x => x.RequirementId == requirementId && x.ScopeId == scopeId, cancellationToken);

        #endregion
    }
}
