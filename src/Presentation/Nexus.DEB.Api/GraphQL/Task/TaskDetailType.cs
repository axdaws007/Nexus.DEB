using Microsoft.AspNetCore.Mvc.Filters;
using Nexus.DEB.Application.Common.Models;

namespace Nexus.DEB.Api.GraphQL
{
	public class TaskDetailType : EntityType<TaskDetail>
	{
		protected override void Configure(IObjectTypeDescriptor<TaskDetail> descriptor)
		{
			descriptor.Field(x => x.ActivityId).ID(nameof(WorkflowActivity));
			descriptor.Field(x => x.TaskTypeId).ID(nameof(FilterItem));
			base.Configure(descriptor);
		}
	}
}
