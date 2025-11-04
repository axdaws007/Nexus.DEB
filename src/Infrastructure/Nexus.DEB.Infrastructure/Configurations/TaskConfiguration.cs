using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus.DEB.Domain.Models;
using Nexus.DEB.Domain.Models.Common;
using Task = Nexus.DEB.Domain.Models.Task;

namespace Nexus.DEB.Infrastructure.Configurations
{
    public class TaskConfiguration : IEntityTypeConfiguration<Task>
    {
        public void Configure(EntityTypeBuilder<Task> builder)
        {
            builder.ToTable("Task", "deb");

            builder.HasBaseType<EntityHead>();

            builder.Property(x => x.EntityId);

            builder
                .HasOne<TaskType>(x => x.TaskType)
                .WithMany()
                .HasForeignKey(x => x.TaskTypeId)
                .OnDelete(DeleteBehavior.NoAction);

            builder
                .HasOne<Statement>(x => x.Statement)
                .WithMany(x => x.Tasks)
                .HasForeignKey(x => x.StatementId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
