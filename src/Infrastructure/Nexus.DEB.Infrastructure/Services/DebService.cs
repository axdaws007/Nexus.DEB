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
        private readonly ICorrelationIdAccessor _correlationIdAccessor;
        private readonly IDateTimeProvider _dateTimeProvider;

        protected readonly IHttpContextAccessor _httpContextAccessor;

        // Constructor
        public DebService(IDbContextFactory<DebContext> dbContextFactory, ICurrentUserService currentUserService, IHttpContextAccessor httpContextAccessor, ICorrelationIdAccessor correlationIdAccessor, IDateTimeProvider dateTimeProvider)
        {
            _dbContext = dbContextFactory.CreateDbContext();
            _currentUserService = currentUserService;
            _httpContextAccessor = httpContextAccessor;
            _correlationIdAccessor = correlationIdAccessor;
            _dateTimeProvider = dateTimeProvider;
        }

        // Dispose
        public ValueTask DisposeAsync() => _dbContext.DisposeAsync();
    }
}
