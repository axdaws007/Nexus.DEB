using Nexus.DEB.Domain.Models;

namespace Nexus.DEB.Api.GraphQL
{
    public class ScopeSummaryType : EntityType<ScopeSummary>
    {
        public ScopeSummaryType() { }

        protected override void Configure(IObjectTypeDescriptor<ScopeSummary> descriptor)
        {
            descriptor.Field(x => x.OwnedById).Ignore();
			descriptor.Field(x => x.EntityTypeTitle).Ignore();
			base.Configure(descriptor);
        }
    }
}
