using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Nexus.DEB.Application.Common.Interfaces;
using Nexus.DEB.Domain.Interfaces;
using Nexus.DEB.Domain.Models;
using Nexus.DEB.Domain.Models.Common;
using Nexus.DEB.Domain.Models.Other;
using Nexus.DEB.Domain.Models.Views;
using Nexus.DEB.Infrastructure.Services;
using Task = System.Threading.Tasks.Task;

namespace Nexus.DEB.Infrastructure.Persistence
{
    public class DebContext : DbContext, IDebContext
    {
        public Guid EventId { get; } = Guid.NewGuid();
		public string? UserDetails { get; protected set; }
		public DebContext(DbContextOptions<DebContext> options)
        : base(options)
        {
            
		}

		public async Task SetFormattedUser()
		{
            if (!string.IsNullOrEmpty(UserDetails))
                return;

            var currentUserService = this.GetService<ICurrentUserService>();
			var userdetails = await currentUserService.GetUserDetailsAsync();
            if (userdetails != null)
            {
                UserDetails = string.Format("{0} ({1} {2})", userdetails.PostTitle, userdetails.FirstName, userdetails.LastName);
            }
            return;
		}

		// Lookups
		public DbSet<CommentType> CommentTypes { get; set; }
        public DbSet<RequirementCategory> RequirementCategories { get; set; }
        public DbSet<RequirementType> RequirementTypes { get; set; }
        public DbSet<Standard> Standards { get; set; }
        public DbSet<TaskType> TaskTypes { get; set; }

        // Linking
        public DbSet<SectionRequirement> SectionRequirements { get; set; }
        public DbSet<StatementRequirementScope> StatementsRequirementsScopes { get; set; }

        // Other
        public DbSet<ModuleSetting> ModuleSettings { get; set; }
        public DbSet<Section> Sections { get; set; }
        public DbSet<SettingsType> SettingsTypes { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<SerialNumber> SerialNumbers { get; set; }
        public DbSet<ChangeRecord> ChangeRecords { get; set; }
        public DbSet<ChangeRecordItem> ChangeRecordItems { get; set; }
        public DbSet<SavedSearch> SavedSearches { get; set; }
        public DbSet<DashboardInfo> DashboardInfos { get; set; }

        // Entities
        public DbSet<EntityHead> EntityHeads { get; set; }
        public DbSet<Requirement> Requirements { get; set; }
        public DbSet<Scope> Scopes { get; set; }
        public DbSet<StandardVersion> StandardVersions { get; set; }
        public DbSet<Statement> Statements { get; set; }
        public DbSet<Domain.Models.Task> Tasks { get; set; }

        // Views
        public DbSet<GroupUser> GroupUsers { get; set; }
        public DbSet<EntityHeadDetail> EntityHeadDetails { get; set; }
        public DbSet<StandardVersionSummary> StandardVersionSummaries { get; set; }
        public DbSet<StandardVersionExport> StandardVersionExport { get; set; }
		public DbSet<StandardVersionDetailView> StandardVersionDetails { get; set; }
        public DbSet<ScopeDetailView> ScopeDetails { get; set; }
		public DbSet<ScopeSummary> ScopeSummaries { get; set; }
        public DbSet<ScopeExport> ScopeExport { get; set; }
        public DbSet<RequirementExport> RequirementExport { get; set; }
        public DbSet<StandardVersionRequirement> StandardVersionRequirements { get; set; }
		public DbSet<StatementExport> StatementExport { get; set; }
        public DbSet<StatementDetailView> StatementDetails { get; set; }
        public DbSet<TaskSummary> TaskSummaries { get; set; }
        public DbSet<TaskExport> TaskExport { get; set; }
        public DbSet<TaskDetailView> TaskDetails { get; set; }
		public DbSet<PawsState> PawsStates { get; set; }
        public DbSet<CommentDetail> CommentDetails { get; set; }
        public DbSet<ViewPost> ViewPosts { get; set; }
        public DbSet<PawsEntityDetail> PawsEntityDetails { get; set; }
        public DbSet<UserAndPost> UsersAndPosts { get; set; }

        // Stored procedures
        public DbSet<MyWorkSummaryItem> MyWorkSummaryItems => Set<MyWorkSummaryItem>();
        public DbSet<MyWorkActivity> MyWorkActivities=> Set<MyWorkActivity>();
        public DbSet<MyWorkDetailItem> MyWorkDetailItems => Set<MyWorkDetailItem>();


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(DebContext).Assembly);
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            BeforeSaveChanges();

            return await base.SaveChangesAsync(cancellationToken);
        }

        private void BeforeSaveChanges()
        {
            var currentUserService = this.GetService<ICurrentUserService>();
            var dateTimeProvider = this.GetService<IDateTimeProvider>();

            var postId = currentUserService.PostId;

            var changeTrackerEntries = this.ChangeTracker
                .Entries()
                .Where(e => (e.Entity is IEntityHead) && (
                  e.State == EntityState.Added ||
                  e.State == EntityState.Modified ||
                  e.State == EntityState.Deleted));

            foreach (var modifiedEntry in changeTrackerEntries)
            {
                if (modifiedEntry != null && modifiedEntry.Entity != null)
                {
                    IEntityHead modifiedEntity = (IEntityHead)modifiedEntry.Entity;

                    modifiedEntity.LastModifiedDate = dateTimeProvider.Now;
                    modifiedEntity.LastModifiedById = postId;

                    if (modifiedEntry.State == EntityState.Added)
                    {
                        var applicationSettingsService = this.GetService<IApplicationSettingsService>();

                        modifiedEntity.CreatedDate = dateTimeProvider.Now;
                        modifiedEntity.CreatedById = postId;
                        modifiedEntity.ModuleId = applicationSettingsService.GetModuleId("DEB");
                    }
                }
            }

        }
    }
}
