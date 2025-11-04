using Microsoft.EntityFrameworkCore;
using Nexus.DEB.Application.Common.Interfaces;
using Nexus.DEB.Infrastructure.Persistence;

namespace Nexus.DEB.Infrastructure.Services
{
    public partial class DebService : IDebService, IAsyncDisposable
    {
        private readonly DebContext _dbContext;

        // Constructor
        public DebService(IDbContextFactory<DebContext> dbContextFactory) => _dbContext = dbContextFactory.CreateDbContext();

        // Dispose
        public ValueTask DisposeAsync() => _dbContext.DisposeAsync();
    }
}
