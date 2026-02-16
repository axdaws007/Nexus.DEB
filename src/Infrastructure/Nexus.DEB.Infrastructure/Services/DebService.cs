using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Nexus.DEB.Application.Common.Interfaces;
using Nexus.DEB.Infrastructure.Persistence;

namespace Nexus.DEB.Infrastructure.Services
{
    public partial class DebService : IDebService, IAsyncDisposable
    {
        private readonly DebContext _dbContext;
        private readonly ICurrentUserService _currentUserService;
		protected readonly IHttpContextAccessor _httpContextAccessor;

		// Constructor
		public DebService(IDbContextFactory<DebContext> dbContextFactory, ICurrentUserService currentUserService, IHttpContextAccessor httpContextAccessor)
        {
			_dbContext = dbContextFactory.CreateDbContext();
			_currentUserService = currentUserService;
			_httpContextAccessor = httpContextAccessor;
		}

        // Dispose
        public ValueTask DisposeAsync() => _dbContext.DisposeAsync();
    }
}
