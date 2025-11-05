using Nexus.DEB.Domain.Models;

namespace Nexus.DEB.Api.GraphQL
{
    public class TaskSummaryType : ObjectType<TaskSummary>
    {
        protected override void Configure(IObjectTypeDescriptor<TaskSummary> descriptor)
        {
            descriptor
                .Field("ownedBy")
                .ResolveWith<CisResolver>(context => context.GetCisOwnedByNameAsync(default, default, default));
            
            base.Configure(descriptor);
        }
    }
}
