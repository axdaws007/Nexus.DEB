using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus.DEB.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nexus.DEB.Infrastructure.Configurations
{
	public class TaskDetailViewConfiguration : IEntityTypeConfiguration<TaskDetailView>
	{
		public void Configure(EntityTypeBuilder<TaskDetailView> builder)
		{
			builder.ToView("vw_TaskDetail", "deb");

			builder.HasNoKey();

			builder.Property(e => e.EntityId)
				.HasColumnName("EntityId")
				.IsRequired();

			builder.Property(e => e.Title)
				.HasColumnName("Title")
				.IsRequired();

			builder.Property(e => e.Description)
				.HasColumnName("Description");

			builder.Property(e => e.SerialNumber)
				.HasColumnName("SerialNumber");

			builder.Property(e => e.CreatedDate)
				.HasColumnName("CreatedDate")
				.IsRequired();

			builder.Property(e => e.LastModifiedDate)
				.HasColumnName("LastModifiedDate")
				.IsRequired();

			builder.Property(e => e.IsRemoved)
				.HasColumnName("IsRemoved")
				.IsRequired();

			builder.Property(e => e.IsArchived)
				.HasColumnName("IsArchived")
				.IsRequired();

			builder.Property(e => e.EntityTypeTitle)
				.HasColumnName("EntityTypeTitle")
				.IsRequired();

			builder.Property(e => e.DueDate)
				.HasColumnName("DueDate");

			builder.Property(e => e.TaskTypeId)
				.HasColumnName("TaskTypeId")
				.IsRequired();

			builder.Property(e => e.TaskType)
				.HasColumnName("TaskType")
				.IsRequired();

			builder.Property(e => e.CreatedBy)
				.HasColumnName("CreatedBy");

			builder.Property(e => e.LastModifiedBy)
				.HasColumnName("LastModifiedBy");

			builder.Property(e => e.OwnedBy)
				.HasColumnName("OwnedBy");

			builder.Property(e => e.OwnedById)
				.HasColumnName("OwnedById");

			builder.Property(e => e.ActivityId)
				.HasColumnName("ActivityID");

			builder.Property(e => e.Status)
				.HasColumnName("Status");

			builder.Property(e => e.StatementId)
				.HasColumnName("StatementId")
				.IsRequired();

			builder.Property(e => e.StatementTitle)
				.HasColumnName("StatementTitle")
				.IsRequired();

			builder.Property(e => e.StatementSerialNumber)
				.HasColumnName("StatementSerialNumber")
				.IsRequired();
		}
	}
}
