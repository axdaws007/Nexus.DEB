using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus.DEB.Domain.Models;

namespace Nexus.DEB.Infrastructure.Configurations
{
    public class GroupUserConfiguration : IEntityTypeConfiguration<GroupUser>
    {
        public void Configure(EntityTypeBuilder<GroupUser> builder)
        {
            // Map to the database view
            builder.ToView("XDB_CIS_Group_User", "common");

            // This is a read-only view, so it has no key
            // We'll use a composite "key" for EF Core tracking purposes
            builder.HasNoKey();

            // Column mappings (these should match the view column names)
            builder.Property(e => e.EntityId)
                .HasColumnName("EntityID")
                .IsRequired();

            builder.Property(e => e.EntityType)
                .HasColumnName("EntityType")
                .IsRequired();

            builder.Property(e => e.Name)
                .HasColumnName("Name");

            builder.Property(e => e.UserFirstName)
                .HasColumnName("UserFirstName");

            builder.Property(e => e.UserLastName)
                .HasColumnName("UserLastName");

            builder.Property(e => e.Email)
                .HasColumnName("Email");

            builder.Property(e => e.IsEnabled)
                .HasColumnName("IsEnabled");

            builder.Property(e => e.IsDeleted)
                .HasColumnName("IsDeleted")
                .IsRequired();
        }
    }
}
