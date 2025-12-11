using Nexus.DEB.Application.Common.Models;

namespace Nexus.DEB.Api.GraphQL
{
	public class TaskDetailType : EntityType<TaskDetail>
	{
		protected override void Configure(IObjectTypeDescriptor<TaskDetail> descriptor)
		{
			base.Configure(descriptor);
		}
	}
}
