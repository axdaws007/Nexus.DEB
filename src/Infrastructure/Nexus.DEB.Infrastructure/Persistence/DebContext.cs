using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using Nexus.DEB.Application.Common.Interfaces;
using Nexus.DEB.Domain.Models;
using Nexus.DEB.Domain.Models.Common;
using Nexus.DEB.Domain.Models.Other;

namespace Nexus.DEB.Infrastructure.Persistence
{
    public class DebContext : DbContext, IDebContext
    {
        public DebContext(DbContextOptions<DebContext> options)
        : base(options)
        {
        }

        // Lookups
        public DbSet<CommentType> CommentTypes { get; set; }
        public DbSet<RequirementCategory> RequirementCategories { get; set; }
        public DbSet<RequirementType> RequirementTypes { get; set; }
        public DbSet<Standard> Standards { get; set; }
        public DbSet<TaskType> TaskTypes { get; set; }

        // Linking
        DbSet<SectionRequirement> SectionRequirements { get; set; }

        // Other
        public DbSet<ModuleSetting> ModuleSettings { get; set; }
        public DbSet<Section> Sections { get; set; }
        public DbSet<SettingsType> SettingsTypes { get; set; }
        public DbSet<Comment> Comments { get; set; }

        // Entities
        public DbSet<EntityHead> EntityHeads { get; set; }
        public DbSet<Requirement> Requirements { get; set; }
        public DbSet<Scope> Scopes { get; set; }
        public DbSet<StandardVersion> StandardVersions { get; set; }
        public DbSet<Statement> Statements { get; set; }
        public DbSet<Domain.Models.Task> Tasks { get; set; }

        // Views
        public DbSet<EntityHeadDetail> EntityHeadDetails { get; set; }
        public DbSet<StandardVersionSummary> StandardVersionSummaries { get; set; }
        public DbSet<StandardVersionExport> StandardVersionExport { get; set; }
        public DbSet<ScopeSummary> ScopeSummaries { get; set; }
        public DbSet<ScopeExport> ScopeExport { get; set; }
//        public DbSet<RequirementSummary> RequirementSummaries { get; set; }
        public DbSet<RequirementExport> RequirementExport { get; set; }
        public DbSet<StatementSummary> StatementSummaries { get; set; }
        public DbSet<StatementExport> StatementExport { get; set; }
        public DbSet<StatementDetail> StatementDetails { get; set; }
        public DbSet<TaskSummary> TaskSummaries { get; set; }
        public DbSet<TaskExport> TaskExport { get; set; }
        public DbSet<PawsState> PawsStates { get; set; }
        public DbSet<CommentDetail> CommentDetails { get; set; }

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
