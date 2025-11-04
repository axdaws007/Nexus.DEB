using Nexus.DEB.Domain.Models;

namespace Nexus.DEB.Api.GraphQL
{
    public class RequirementSummaryType : ObjectType<RequirementSummary>
    {
        protected override void Configure(IObjectTypeDescriptor<RequirementSummary> descriptor)
        {
            descriptor
                .Field("status")
                .ResolveWith<PawsResolver>(context => context.GetCurrentPawsStatusAsync(default, default, default));

            base.Configure(descriptor);
        }
    }
}
