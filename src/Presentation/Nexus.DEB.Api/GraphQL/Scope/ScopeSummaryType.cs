using Nexus.DEB.Domain.Models;

namespace Nexus.DEB.Api.GraphQL
{
    public class ScopeSummaryType : ObjectType<ScopeSummary>
    {
        public ScopeSummaryType() { }

        protected override void Configure(IObjectTypeDescriptor<ScopeSummary> descriptor)
        {
            descriptor
                .Field("ownedBy")
                .ResolveWith<CisResolver>(context => context.GetCisOwnedByNameAsync(default, default, default));

            descriptor
                .Field("status")
                .ResolveWith<PawsResolver>(context => context.GetCurrentPawsStatusAsync(default, default, default));

            base.Configure(descriptor);
        }
    }
}
