using Nexus.DEB.Domain.Models;

namespace Nexus.DEB.Api.GraphQL
{
    public class TaskSummaryType : EntityType<TaskSummary>
    {
        protected override void Configure(IObjectTypeDescriptor<TaskSummary> descriptor)
        {
            descriptor.Field(x => x.OwnedById).Ignore();

            base.Configure(descriptor);
        }
    }
}
