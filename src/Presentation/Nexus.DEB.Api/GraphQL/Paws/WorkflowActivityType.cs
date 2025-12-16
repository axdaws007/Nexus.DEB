using Nexus.DEB.Application.Common.Models;

namespace Nexus.DEB.Api.GraphQL.Paws
{
    public class WorkflowActivityType : ObjectType<WorkflowActivity>
    {
        protected override void Configure(IObjectTypeDescriptor<WorkflowActivity> descriptor)
        {
            descriptor.Field(x => x.ActivityID).ID();
        }
    }
}
