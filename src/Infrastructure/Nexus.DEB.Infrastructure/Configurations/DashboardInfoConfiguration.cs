using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus.DEB.Domain.Models.Other;

namespace Nexus.DEB.Infrastructure.Configurations
{
    public class DashboardInfoConfiguration : IEntityTypeConfiguration<DashboardInfo>
    {
        public void Configure(EntityTypeBuilder<DashboardInfo> builder)
        {
            builder.ToTable("DashboardInfo", "common");

            builder.HasKey(x => x.EntityId);
        }
    }
}
