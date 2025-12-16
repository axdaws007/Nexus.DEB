using Microsoft.AspNetCore.Mvc.Filters;
using Nexus.DEB.Application.Common.Models;

namespace Nexus.DEB.Api.GraphQL
{
	public class TaskDetailType : EntityType<TaskDetail>
	{
		protected override void Configure(IObjectTypeDescriptor<TaskDetail> descriptor)
		{
			descriptor.Field(x => x.ActivityId).ID("Activity");
			descriptor.Field(x => x.TaskTypeId).ID("FilterItem");
			base.Configure(descriptor);
		}
	}
}
