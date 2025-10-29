using Microsoft.EntityFrameworkCore;
using Nexus.DEB.Application.Common.Interfaces;

namespace Nexus.DEB.Infrastructure.Persistence
{
    public class DebContext : DbContext, IDebContext
    {
        public DebContext(DbContextOptions<DebContext> options)
        : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(DebContext).Assembly);
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return base.SaveChangesAsync(cancellationToken);
        }
    }
}
