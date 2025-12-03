using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus.DEB.Domain.Models;
using Nexus.DEB.Domain.Models.Common;

namespace Nexus.DEB.Infrastructure.Configurations
{
    public class StatementConfiguration : IEntityTypeConfiguration<Statement>
    {
        public void Configure(EntityTypeBuilder<Statement> builder)
        {
            builder.ToTable("Statement", "deb", t => { t.HasTrigger("Statement_ChangeTracking"); });

            builder.HasBaseType<EntityHead>();

            builder
                .HasMany<Domain.Models.Task>(x => x.Tasks)
                .WithOne(x => x.Statement)
                .HasForeignKey(x => x.StatementId);

        }
    }
}