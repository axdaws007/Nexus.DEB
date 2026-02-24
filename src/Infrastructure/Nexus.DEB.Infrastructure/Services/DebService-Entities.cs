using Mapster;
using Microsoft.EntityFrameworkCore;
using Nexus.DEB.Application.Common.Extensions;
using Nexus.DEB.Application.Common.Models;
using Nexus.DEB.Application.Common.Models.Filters;
using Nexus.DEB.Domain.Interfaces;
using Nexus.DEB.Domain.Models;
using Nexus.DEB.Domain.Models.Common;
using Scope = Nexus.DEB.Domain.Models.Scope;

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

        public async Task<IReadOnlyList<IEntityHead>> GetEntityHeadsAsync(CancellationToken cancellationToken)
        {
            var requirements = await _dbContext.Requirements.AsNoTracking().Cast<IEntityHead>().ToListAsync(cancellationToken);
            var statements = await _dbContext.Statements.AsNoTracking().Cast<IEntityHead>().ToListAsync(cancellationToken);
            var tasks = await _dbContext.Tasks.AsNoTracking().Cast<IEntityHead>().ToListAsync(cancellationToken);
            var scopes = await _dbContext.Scopes.AsNoTracking().Cast<IEntityHead>().ToListAsync(cancellationToken);
            var standardVersions = await _dbContext.StandardVersions.AsNoTracking().Cast<IEntityHead>().ToListAsync(cancellationToken);

            return requirements
                .Concat(statements)
                .Concat(tasks)
                .Concat(scopes)
                .Concat(standardVersions)
                .ToList();
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
                    Description = r.Description,
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
                    query = query.Where(r => 
                        r.Title.Contains(filters.SearchText) || 
                        (r.Description != null && r.Description.Contains(filters.SearchText)) || 
                        (r.SerialNumber != null && r.SerialNumber.Contains(filters.SearchText)));
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

                if (filters.OnlyShowAvailableRequirementScopeCombinations)
                {
                    var requirementIdsWithAvailableCombinations = _dbContext.Set<Requirement>()
                        .Where(r => r.Scopes.Any(s =>
                            (filters.ScopeIds == null || filters.ScopeIds.Count == 0 || filters.ScopeIds.Contains(s.EntityId)) &&

                            // This scope (from the requirement's available scopes) hasn't been allocated yet
                            !r.StatementsRequirementsScopes.Any(srs => srs.ScopeId == s.EntityId)))
                        .Select(r => r.EntityId);

                    query = query.Where(r => requirementIdsWithAvailableCombinations.Contains(r.EntityId));
                }
            }

            return query;
        }

        public async Task<IEnumerable<StandardVersionRequirementDetail>> GetStandardVersionRequirementsForGridAsync(StandardVersionRequirementsFilters? filters, CancellationToken cancellationToken)
		{
            var query = from svr in _dbContext.StandardVersionRequirements.AsNoTracking()
                        join r in (_dbContext.Requirements.AsNoTracking().Include(r => r.Scopes).Include(r => r.StandardVersions)) on svr.RequirementId equals r.EntityId
                        where filters == null || !filters.StandardVersionId.HasValue || (svr.StandardVersionId == filters.StandardVersionId.Value)
                        select new StandardVersionRequirementDetail
                        {
                            RequirementId = svr.RequirementId,
                            SerialNumber = svr.SerialNumber,
                            Title = svr.Title,
                            Description = svr.Description,
                            SectionId = svr.SectionId,
                            Section = svr.Section,
                            OtherScopes = r.Scopes.Where(w => filters == null || w.EntityId != filters.ScopeId).Count(),
                            IncludedInScope = filters != null ? r.Scopes.Any(a => a.EntityId == filters.ScopeId) : false
                        };

			if (filters != null)
            {
                if (filters.SectionId.HasValue)
                {
                    query = query.Where(w => w.SectionId == filters.SectionId.Value);
                }

                if (!string.IsNullOrWhiteSpace(filters.SearchText))
                {
                    query = query.Where(w => 
                        w.Title.Contains(filters.SearchText) || 
                        w.SerialNumber.Contains(filters.SearchText) || 
                        (w.Description != null && w.Description.Contains(filters.SearchText)));
                }
            }

			/****************************************************************************
             * This may not be the most performant way of getting the list of Standard  *
             * Version Ids for each Requirement. Originally wanted to get the list of   *
             * StandardVersionIds for each requirement in the original query, but this  *
             * was causing issues with EF Core translating to SQL. This should be       *
             * revisited at a later date to see if it can be optimised. 09/02/26        *
             ****************************************************************************/
			var allStandardVersionRequirements = _dbContext.StandardVersionRequirements;

            return query.ToList().Select(s => new StandardVersionRequirementDetail
            {
                RequirementId = s.RequirementId,
                SerialNumber = s.SerialNumber,
                Title = s.Title,
                Description = s.Description,
                SectionId = s.SectionId,
                Section = s.Section,
                OtherScopes = s.OtherScopes,
                IncludedInScope = s.IncludedInScope,
                StandardVersionIds = allStandardVersionRequirements.Where(w => w.RequirementId == s.RequirementId).Select(s => s.StandardVersionId).Distinct().ToList()
            });
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
                    query = query.Where(r => 
                        r.Title.Contains(filters.SearchText) || 
                        (r.Description != null && r.Description.Contains(filters.SearchText)) ||
                        (r.SerialNumber != null && r.SerialNumber.Contains(filters.SearchText)));
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

                if (filters.SortBy != null)
                {
                    query = query.ApplySorting(filters.SortBy, new Dictionary<string, string>
                    {
                        ["serialNumber"] = "SerialNumber",
                        ["title"] = "Title",
                        ["status"] = "Status",
                        ["lastModifiedDate"] = "LastModifiedDate"
                    });
                }
            }

            return query;
        }

        public async Task<Requirement?> GetRequirementByIdAsync(Guid id, CancellationToken cancellationToken)
            => await _dbContext.Requirements.FirstOrDefaultAsync(x => x.EntityId == id, cancellationToken);


        public async Task<RequirementDetail?> GetRequirementDetailByIdAsync(Guid id, CancellationToken cancellationToken)
		{
            var requirement = await _dbContext.RequirementDetails.AsNoTracking()
                .Include(r => r.RequirementType)
                .Include(r => r.RequirementCategory)
                .FirstOrDefaultAsync(s => s.EntityId == id, cancellationToken);

			if (requirement == null)
				return null;

			var requirementDetail = requirement.Adapt<RequirementDetail>();

            requirementDetail.StandardVersionSections = GetRelatedStandardVersionsAndSections(id);
			requirementDetail.ScopeStatements = GetRelatedScopesWithStatements(id);
            
            return requirementDetail;
		}

        public ICollection<StandardVersionWithSections> GetRelatedStandardVersionsAndSections(Guid requirementId)
		{
			var sectionsAndStandardVersions = _dbContext.Sections.AsNoTracking()
                .Include(r => r.StandardVersion)
                .Where(w => w.SectionRequirements.Any(a => a.RequirementID == requirementId) && w.StandardVersion.Requirements.Any(a => a.EntityId == requirementId));

            var standardVersions = sectionsAndStandardVersions.Select(s => s.StandardVersion).Distinct();

            var standardVersionSections = new List<StandardVersionWithSections>();
			foreach (var sv in standardVersions)
			{
				standardVersionSections.Add(new StandardVersionWithSections
                {
                    EntityId = sv.EntityId,
					Title = sv.Title,
					SerialNumber = sv.SerialNumber,
                    Sections = sectionsAndStandardVersions
						.Where(w => w.StandardVersion.EntityId == sv.EntityId)
						.Distinct()
						.ToList()
				});
			}

            return standardVersionSections;
		}

		public ICollection<ScopeWithStatements> GetRelatedScopesWithStatements(Guid requirementId)
		{
			var statementRequirementScopes = _dbContext.StatementsRequirementsScopes
				.AsNoTracking()
				.Where(srs => srs.RequirementId == requirementId)
				.Include(srs => srs.Statement)
				.Include(srs => srs.Scope);

			var scopes = statementRequirementScopes.Select(srs => srs.Scope).Distinct();

			var scopesWithStatements = new List<ScopeWithStatements>();
			foreach (var sc in scopes)
			{
				scopesWithStatements.Add(new ScopeWithStatements
				{
					EntityId = sc.EntityId,
					Title = sc.Title,
					SerialNumber = sc.SerialNumber,
					Statements = statementRequirementScopes
						.Where(w => w.Scope.EntityId == sc.EntityId)
						.Select(srs => srs.Statement)
						.Distinct()
						.ToList()
				});
			}

			return scopesWithStatements;
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

        public async Task<Requirement> CreateRequirementAsync(
            Requirement requirement,
            CancellationToken cancellationToken = default)
        {
            await _dbContext.Requirements.AddAsync(requirement, cancellationToken);

            await _dbContext.SaveChangesAsync(cancellationToken);

            return requirement;
        }

        public async Task<Requirement> UpdateRequirementAsync(
            Requirement requirement,
            CancellationToken cancellationToken = default)
        {
            await _dbContext.SaveChangesAsync(cancellationToken);

            return requirement;
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
                              IsEnabled = !sc.IsRemoved,
                              EntityType = sc.EntityTypeTitle
                          }).ToListAsync(cancellationToken);
        }


        public IQueryable<ScopeSummary> GetScopesForGrid(ScopeFilters? filters)
        {
            var query = _dbContext.ScopeSummaries.AsNoTracking();

            if (filters != null)
            {
                if (filters.StandardVersionIds != null && filters.StandardVersionIds.Count > 0)
                {
                    var scopeIds = _dbContext.Set<Scope>()
                        .Where(s => s.Requirements
                            .Any(svr => svr.StandardVersions
                                .Any(sv => filters.StandardVersionIds.Contains(sv.EntityId))))
                        .Select(s => s.EntityId);

                    query = query.Where(s => scopeIds.Contains(s.EntityId));
                }

                if (filters.StatusIds != null && filters.StatusIds.Count > 0)
                {
                    query = query.Where(x => filters.StatusIds.Contains(x.StatusId));
                }
            }

            return query;
        }

        public IQueryable<ScopeExport> GetScopesForExport(ScopeFilters? filters)
        {
            var query = _dbContext.ScopeExport.AsNoTracking();

            if (filters != null)
            {
                if (filters.StandardVersionIds != null && filters.StandardVersionIds.Count > 0)
                {
                    var scopeIds = _dbContext.Set<Scope>()
                        .Where(s => s.Requirements
                            .Any(svr => svr.StandardVersions
                                .Any(sv => filters.StandardVersionIds.Contains(sv.EntityId))))
                        .Select(s => s.EntityId);

                    query = query.Where(s => scopeIds.Contains(s.EntityId));
                }

                if (filters.StatusIds != null && filters.StatusIds.Count > 0)
                {
                    query = query.Where(x => filters.StatusIds.Contains(x.StatusId));
                }

                if (filters.SortBy != null)
                {
                    query = query.ApplySorting(filters.SortBy, new Dictionary<string, string>
                    {
                        ["title"] = "Title",
                        ["status"] = "Status",
                        ["ownedBy"] = "OwnedBy",
                        ["createdDate"] = "CreatedDate",
                        ["lastModifiedDate"] = "LastModifiedDate",
                        ["numberOfLinkedStandardVersions"] = "NumberOfLinkedStandardVersions"
                    });
                }
            }

            return query;
        }

		public async Task<Scope?> GetScopeByIdAsync(Guid id, CancellationToken cancellationToken)
			=> await _dbContext.Scopes.FirstOrDefaultAsync(x => x.EntityId == id, cancellationToken);

		public async Task<ScopeDetail?> GetScopeDetailByIdAsync(Guid id, CancellationToken cancellationToken)
		{
            var scope = await _dbContext.ScopeDetails.AsNoTracking()
                .FirstOrDefaultAsync(s => s.EntityId == id, cancellationToken);

			if (scope == null)
				return null;

			var scopeDetail = scope.Adapt<ScopeDetail>();

            var scopeRequirements = _dbContext.Requirements.Include(r => r.StandardVersions).Where(w => w.Scopes.Any(a => a.EntityId == scope.EntityId));
            scopeDetail.RequirementIds = scopeRequirements.Select(s => s.EntityId).ToList();
            scopeDetail.StandardVersionRequirements.AddRange(await GetStandardVersionRequirementsForScopeAsync(scope.EntityId, cancellationToken));

			return scopeDetail;
		}

        public async Task<List<StandardVersionRequirements>> GetStandardVersionRequirementsForScopeAsync(Guid scopeId, CancellationToken cancellationToken)
		{
			var scopeRequirements = _dbContext.Requirements.Include(r => r.StandardVersions).Where(w => w.Scopes.Any(a => a.EntityId == scopeId));
			var standardVersionIds = scopeRequirements.SelectMany(s => s.StandardVersions).Distinct().Select(s => s.EntityId);
			var standardVersions = _dbContext.StandardVersions.AsNoTracking().Include(sv => sv.Standard).Include(sv => sv.Requirements)
                /*.Where(w => standardVersionIds.Contains(w.EntityId))*/;
			var standardVersionStates = _dbContext.PawsEntityDetails.AsNoTracking().Where(w => standardVersionIds.Contains(w.EntityId));

            var standardVersionRequirementsList = new List<StandardVersionRequirements>();
			foreach (var sv in standardVersions)
			{
				var svR = new StandardVersionRequirements();
				svR.StandardVersionId = sv.EntityId;
				svR.StandardVersionTitle = sv.Title;
				svR.Status = standardVersionStates.FirstOrDefault(s => s.EntityId == sv.EntityId)?.PseudoStateTitle ?? string.Empty;
				svR.TotalRequirements = sv.Requirements.Count;
				svR.TotalRequirementsInScope = scopeRequirements.Where(w => w.StandardVersions.Any(a => a.EntityId == sv.EntityId)).Count();

				standardVersionRequirementsList.Add(svR);
			}

			return standardVersionRequirementsList;
		}

		public async Task<ScopeChildCounts> GetChildCountsForScopeAsync(Guid id, CancellationToken cancellationToken)
		{
			var numberOfComments = await GetCommentsCountForEntityAsync(id, cancellationToken);

			var numberOfHistoryEvents = await GetChangeRecordsCountForEntityAsync(id, cancellationToken);

            var scope = await _dbContext.Scopes.Include(s => s.Requirements).FirstOrDefaultAsync(x => x.EntityId == id, cancellationToken);
            
            var requirementsCount = 0;
            if (scope != null)
            {
                requirementsCount = scope.Requirements.Count();
            }

			// Note: AttachmentsCount populated in the GraphQL query as we're interrogating the DMS Web API.

			return new ScopeChildCounts()
			{
				CommentsCount = numberOfComments,
				HistoryCount = numberOfHistoryEvents,
				RequirementsCount = requirementsCount
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

		public async Task<Scope> CreateScopeAsync(
			Scope scope,
			CancellationToken cancellationToken = default)
		{
			await _dbContext.Scopes.AddAsync(scope, cancellationToken);

			await _dbContext.SaveChangesAsync(cancellationToken);

			return scope;
		}

		public async Task<Scope> UpdateScopeAsync(
			Scope scope,
			CancellationToken cancellationToken = default)
		{
			await _dbContext.SaveChangesAsync(cancellationToken);

			return scope;
		}

        public async Task<ScopeDetail?> UpdateScopeRequirementsAsync(Guid scopeId, StandardVersion standardVersion, List<Guid> idsToAdd, List<Guid> idsToRemove, bool addAll, bool removeAll, CancellationToken cancellationToken)
		{
			var scope = _dbContext.Scopes.Include(s => s.Requirements).FirstOrDefault(f => f.EntityId == scopeId);

			if (addAll || removeAll)
			{
                // 1. Get all requirements for the standard version
				var svReqs = _dbContext.Requirements.Where(w => w.StandardVersions.Any(a => a.EntityId == standardVersion.EntityId)).ToList();
				// 2. Remove any in idsToRemove or idsToAdd
				svReqs.RemoveAll(r => addAll ? idsToRemove.Contains(r.EntityId) : idsToAdd.Contains(r.EntityId));

                foreach(var svr in svReqs)
                {
                    if (addAll)
					{
						// 3. Add finalised list to scope, if not already there
						if (!scope.Requirements.Any(a => a.EntityId == svr.EntityId))
						{
							scope.Requirements.Add(svr);
						}
					}
                    else
					{
						// 3. Remove finalised list from scope, if present
						if (scope.Requirements.Any(a => a.EntityId == svr.EntityId))
                        {
                            scope.Requirements.Remove(svr);
                        }
                    }
                }
            }
            else
            {
				// 1. Get all requirements for idsToAdd and idsToRemove
                var svReqs = _dbContext.Requirements.Where(w => idsToAdd.Contains(w.EntityId) || idsToRemove.Contains(w.EntityId)).ToList();
				foreach (var req in svReqs)
				{
					if (idsToAdd.Contains(req.EntityId))
					{
						// 2. Add idsToAdd to scope requirements
						if (!scope.Requirements.Any(a => a.EntityId == req.EntityId))
						{
							scope.Requirements.Add(req);
						}
					}
					else if (idsToRemove.Contains(req.EntityId))
                    {
						// 3. Remove idsToRemove from scope requirements
						if (scope.Requirements.Any(a => a.EntityId == req.EntityId))
						{
							scope.Requirements.Remove(req);
						}
					}
				}
			}

            await _dbContext.SaveChangesAsync(cancellationToken);

            return await GetScopeDetailByIdAsync(scopeId, cancellationToken);
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
                    SerialNumber = s.SerialNumber ?? string.Empty,
                    Title = s.Title,
                    Description = s.Description,
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
                    query = query.Where(s => 
                        s.Title.Contains(filters.SearchText) ||
                        (s.Description != null && s.Description.Contains(filters.SearchText)) ||
                        s.SerialNumber.Contains(filters.SearchText));
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

                // Text search on Title, Description and SerialNumber
                if (!string.IsNullOrWhiteSpace(filters.SearchText))
                {
                    query = query.Where(r => 
                        r.Title.Contains(filters.SearchText) || 
                        (r.Description != null && r.Description.Contains(filters.SearchText)) ||
                        (r.SerialNumber != null && r.SerialNumber.Contains(filters.SearchText)));
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

                if (filters.SortBy != null)
                {
                    query = query.ApplySorting(filters.SortBy, new Dictionary<string, string>
                    {
                        ["serialNumber"] = "SerialNumber",
                        ["title"] = "Title",
                        ["status"] = "Status",
                        ["lastModifiedDate"] = "LastModifiedDate",
                        ["ownedBy"] = "OwnedBy",
                    });
                }
            }

            return query;
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

            statementDetail.LinkedCommonEvidences = _dbContext.EntityDocumentLinking.AsNoTracking()
                                                        .Where(x => x.EntityId == id && x.Context == EntityDocumentLinkingContexts.CommonEvidence)
                                                        .Select(x => x.DocumentId)
                                                        .ToList();

            return statementDetail;
        }

        public async Task<ICollection<Guid>> GetStandardVersionIdsForStatementAsync(Guid statementId, CancellationToken cancellationToken)
            => await _dbContext.Set<StatementRequirementScope>()
                        .Where(srs => srs.StatementId == statementId)
                        .SelectMany(srs => srs.Requirement.StandardVersions)
                        .Select(sv => sv.EntityId)
                        .Distinct()
                        .ToListAsync(cancellationToken);

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
                                      IsEnabled = !sv.IsRemoved,
									  EntityType = sv.EntityTypeTitle
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
                                    .ThenBy(t => t.Reference)
                                    .ThenBy(t => t.Title)
                                    .Select(s => new FilterItemEntity()
                                    {
                                        Id = s.Id,
                                        Value = s.Reference + s.Title,
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

                if (filters.SortBy != null)
                {
                    query = query.ApplySorting(filters.SortBy, new Dictionary<string, string>
                    {
                        ["standardTitle"] = "StandardTitle",
                        ["version"] = "VersionTitle",
                        ["status"] = "Status",
                        ["effectiveFrom"] = "EffectiveStartDate",
                        ["effectiveTo"] = "EffectiveEndDate",
                        ["lastModifiedDate"] = "LastModifiedDate",
                        ["numberOfLinkedScopes"] = "NumberOfLinkedScopes"
                    });
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

        public async Task<int?> GetStandardVersionTotalRequirementsAsync(Guid id, CancellationToken cancellationToken)
        {
            var sv = await _dbContext.StandardVersions.Include(i => i.Requirements)
                .FirstOrDefaultAsync(sv => sv.EntityId == id);
            if(sv != null)
            {
				return sv.Requirements.Count;
			}

			return null;
		}

        public async Task<StandardVersion> CreateStandardVersionAsync(StandardVersion standardVersion, CancellationToken cancellationToken = default)
		{
			await _dbContext.StandardVersions.AddAsync(standardVersion, cancellationToken);
			await _dbContext.SaveChangesAsync(cancellationToken);
			return standardVersion;
		}

        public async Task<StandardVersion> UpdateStandardVersionAsync(StandardVersion standardVersion, CancellationToken cancellationToken = default)
        {
			await _dbContext.SaveChangesAsync(cancellationToken);
			return standardVersion;
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
                    query = query.Where(r => 
                        r.Title.Contains(filters.SearchText) || 
                        (r.Description != null && r.Description.Contains(filters.SearchText)) || 
                        (r.SerialNumber != null && r.SerialNumber.Contains(filters.SearchText)));
                }

                // Date range filter
                if (filters.DueDateFrom.HasValue)
                {
                    query = query.Where(r => r.DueDate >= filters.DueDateFrom.Value);
                }

                if (filters.DueDateTo.HasValue)
                {
                    query = query.Where(r => r.DueDate < filters.DueDateTo.Value.AddDays(1));
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
                    query = query.Where(r =>
                        r.Title.Contains(filters.SearchText) ||
                        (r.Description != null && r.Description.Contains(filters.SearchText)) ||
                        (r.SerialNumber != null && r.SerialNumber.Contains(filters.SearchText)));
                }

                // Date range filter
                if (filters.DueDateFrom.HasValue)
                {
                    query = query.Where(r => r.DueDate >= filters.DueDateFrom.Value);
                }

                if (filters.DueDateTo.HasValue)
                {
                    query = query.Where(r => r.DueDate < filters.DueDateTo.Value.AddDays(1));
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

                if (filters.SortBy != null)
                {
                    query = query.ApplySorting(filters.SortBy, new Dictionary<string, string>
                    {
                        ["serialNumber"] = "SerialNumber",
                        ["title"] = "Title",
                        ["ownedBy"] = "OwnedBy",
                        ["dueDate"] = "DueDate",
                        ["taskTypeTitle"] = "TaskTypeTitle",
                        ["status"] = "Status"
                    });
                }
            }

            return query;
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
