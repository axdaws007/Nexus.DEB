using Microsoft.EntityFrameworkCore;
using Nexus.DEB.Application.Common.Interfaces;
using Nexus.DEB.Application.Common.Models;
using Nexus.DEB.Application.Common.Models.Filters;
using Nexus.DEB.Domain.Models;
using Nexus.DEB.Infrastructure.Persistence;

namespace Nexus.DEB.Infrastructure.Services
{
    public class DebService : IDebService, IAsyncDisposable
    {
        private readonly DebContext _dbContext;

        // Constructor
        public DebService(IDbContextFactory<DebContext> dbContextFactory) => _dbContext = dbContextFactory.CreateDbContext();

        // Dispose
        public ValueTask DisposeAsync() => _dbContext.DisposeAsync();

        // Start of Service methods
        public IQueryable<StandardVersionSummary> GetStandardVersionsForGrid(StandardVersionSummaryFilters? filters)
        {
            var query = _dbContext.StandardVersionSummaries.AsNoTracking();

            if (filters != null)
            {
                if (filters.StandardId.HasValue)
                {
                    query = query.Where(x => x.StandardId == filters.StandardId.Value);
                }

                if (filters.StatusId.HasValue)
                {
                    query = query.Where(x => x.StatusId == filters.StatusId.Value);
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

        public IQueryable<ScopeSummary> GetScopesForGrid() => _dbContext.ScopeSummaries.AsNoTracking();

        public IQueryable<RequirementSummary> GetRequirementsForGrid(RequirementSummaryFilters? filters)
        {
            var query = _dbContext.RequirementSummaries.AsQueryable();

            if (filters != null)
            {
                // Filter by StandardVersion (using navigation property)
                if (filters.StandardVersionId.HasValue)
                {
                    var requirementIds = _dbContext.Set<Requirement>()
                        .Where(r => r.StandardVersions.Any(sv => sv.EntityId == filters.StandardVersionId.Value))
                        .Select(r => r.EntityId);

                    query = query.Where(r => requirementIds.Contains(r.EntityId));
                }

                // Filter by Scope (using navigation property)
                if (filters.ScopeId.HasValue)
                {
                    var requirementIds = _dbContext.Set<Requirement>()
                        .Where(r => r.Scopes.Any(s => s.EntityId == filters.ScopeId.Value))
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
            }

            return query;
        }

        public IQueryable<FilterItem<Guid>> GetScopesForFilter()
        {
            var query = from s in _dbContext.Scopes
                        where s.IsRemoved == false
                        select new FilterItem<Guid>()
                        {
                            Id = s.EntityId,
                            Title = s.Title
                        };

            return query;
        }

        public IQueryable<FilterItem<Guid>> GetStandardVersionsForFilter()
        {
            var query = from sv in _dbContext.StandardVersions.Include(x => x.Standard)
                        where sv.IsRemoved == false
                        select new FilterItem<Guid>()
                        {
                            Id = sv.EntityId,
                            Title = sv.Standard.Title + ":" + sv.MajorVersion + " - " + sv.Title
                        };

            return query;
        }

        public IQueryable<FilterItem<short>> GetStandardsForFilter()
        {
            var query = from s in _dbContext.Standards
                        where s.IsEnabled == true
                        select new FilterItem<short>()
                        {
                            Id = s.Id,
                            Title = s.Title
                        };

            return query;
        }

        public IQueryable<StatementSummary> GetStatementsForGrid(StatementSummaryFilters? filters)
        {
            var query = _dbContext.StatementSummaries.AsQueryable();

            if (filters != null)
            {
                // Filter by StandardVersion (using navigation property)
                if (filters.StandardVersionId.HasValue)
                {
                    var requirementIds = _dbContext.Set<Requirement>()
                        .Where(r => r.StandardVersions.Any(sv => sv.EntityId == filters.StandardVersionId.Value))
                        .Select(r => r.EntityId);

                    query = query.Where(r => requirementIds.Contains(r.EntityId));
                }

                // Filter by Scope (using navigation property)
                if (filters.ScopeId.HasValue)
                {
                    var requirementIds = _dbContext.Set<Requirement>()
                        .Where(r => r.Scopes.Any(s => s.EntityId == filters.ScopeId.Value))
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

                // TODO
                // OwnedById filter
            }

            return query;
        }
    }
}
