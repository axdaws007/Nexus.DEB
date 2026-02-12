using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus.DEB.Domain.Models;

namespace Nexus.DEB.Infrastructure.Configurations
{
    public class EntityDocumentLinkingConfiguration : IEntityTypeConfiguration<EntityDocumentLinking>
    {
        public void Configure(EntityTypeBuilder<EntityDocumentLinking> builder)
        {
            builder.ToTable("EntityDocumentLinking", "common");

            builder.HasKey(x => x.Id);

            builder.HasIndex(x => new { x.EntityId, x.Context });
            builder.HasIndex(x => new { x.LibraryId, x.DocumentId });
        }
    }
}
