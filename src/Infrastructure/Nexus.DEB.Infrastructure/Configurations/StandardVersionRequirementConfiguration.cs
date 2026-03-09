using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus.DEB.Domain.Models;

namespace Nexus.DEB.Infrastructure.Configurations
{
    public class StandardVersionRequirementConfiguration : IEntityTypeConfiguration<StandardVersionRequirement>
    {
        public void Configure(EntityTypeBuilder<StandardVersionRequirement> builder)
        {
            builder.ToTable("StandardVersionRequirement", "deb");

            builder.HasKey(x => new { x.RequirementId, x.StandardVersionId });
        }
    }
}
