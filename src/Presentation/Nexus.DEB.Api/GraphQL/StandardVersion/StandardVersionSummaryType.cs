using Nexus.DEB.Application.Common.Interfaces;
using Nexus.DEB.Domain.Models;

namespace Nexus.DEB.Api.GraphQL.StandardVersion
{
    public class StandardVersionSummaryType : ObjectType<StandardVersionSummary>
    {
        public StandardVersionSummaryType(IPawsService pawsService)
        {
        }

        protected override void Configure(IObjectTypeDescriptor<StandardVersionSummary> descriptor)
        {
            descriptor.Field(x => x.StandardId).Ignore();
            descriptor.Field(x => x.StatusId).Ignore();

            base.Configure(descriptor);
        }
    }
}
