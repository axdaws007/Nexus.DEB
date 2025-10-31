using Microsoft.EntityFrameworkCore;
using Nexus.DEB.Application.Common.Interfaces;
using Nexus.DEB.Domain.Models;
using Nexus.DEB.Infrastructure.Persistence;

namespace Nexus.DEB.Infrastructure.Services
{
    public class DebService : IDebService, IAsyncDisposable
    {
        private readonly DebContext _dbContext;

        public DebService(IDbContextFactory<DebContext> dbContextFactory) => _dbContext = dbContextFactory.CreateDbContext();

        public ValueTask DisposeAsync() => _dbContext.DisposeAsync();

        public IQueryable<StandardVersionSummary> GetStandardVersionsForGrid()
        {
            return _dbContext.StandardVersionSummaries.AsNoTracking();
        }
    }
}
