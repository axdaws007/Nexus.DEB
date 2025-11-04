using Microsoft.EntityFrameworkCore;
using Nexus.DEB.Application.Common.Interfaces;
using Nexus.DEB.Application.Common.Models;
using Nexus.DEB.Application.Common.Models.Filters;
using Nexus.DEB.Domain.Models;
using Nexus.DEB.Infrastructure.Persistence;
using System.Linq;

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

        public IQueryable<Scope> GetScopes()
        {
            var query = from s in _dbContext.Scopes
                        where s.IsRemoved == false
                        select s;

            return query;
        }

        public IQueryable<StandardVersion> GetStandardVersions()
        {
            var query = from sv in _dbContext.StandardVersions
                                        .Include(x => x.Standard)
                        where sv.IsRemoved == false
                        select sv;

            return query;
        }

        public IQueryable<Standard> GetStandards()
        {
            var query = from s in _dbContext.Standards
                        where s.IsEnabled == true
                        select s;

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
