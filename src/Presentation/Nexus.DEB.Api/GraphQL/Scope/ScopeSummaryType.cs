using Nexus.DEB.Domain.Models;

namespace Nexus.DEB.Api.GraphQL
{
    public class ScopeSummaryType : ObjectType<ScopeSummary>
    {
        public ScopeSummaryType() { }

        protected override void Configure(IObjectTypeDescriptor<ScopeSummary> descriptor)
        {
            descriptor.Field(x => x.OwnedById).Ignore();

            base.Configure(descriptor);
        }
    }
}
