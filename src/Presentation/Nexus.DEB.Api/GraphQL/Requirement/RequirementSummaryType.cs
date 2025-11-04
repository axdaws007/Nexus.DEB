using Nexus.DEB.Domain.Models;

namespace Nexus.DEB.Api.GraphQL
{
    public class RequirementSummaryType : ObjectType<RequirementSummary>
    {
        protected override void Configure(IObjectTypeDescriptor<RequirementSummary> descriptor)
        {
            base.Configure(descriptor);
        }
    }
}
