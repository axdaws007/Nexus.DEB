using Nexus.DEB.Application.Common.Models;

namespace Nexus.DEB.Api.GraphQL.Paws
{
    public class WorkflowActivityType : ObjectType<WorkflowActivity>
    {
        protected override void Configure(IObjectTypeDescriptor<WorkflowActivity> descriptor)
        {
            descriptor.Field("isEnabled")
                .Resolve(context =>
                {
                    var parent = context.Parent<WorkflowActivity>();

                    return !parent.IsRemoved;
                });
        }
    }
}
