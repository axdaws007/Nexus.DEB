using Nexus.DEB.Domain.Models;

namespace Nexus.DEB.Api.GraphQL
{
    public class TaskTypeType : ObjectType<TaskType>
    {
        protected override void Configure(IObjectTypeDescriptor<TaskType> descriptor)
        {
            descriptor.Field(x => x.IsEnabled).Ignore();

            base.Configure(descriptor);
        }
    }
}
