using Nexus.DEB.Application.Common.Interfaces;
using Nexus.DEB.Domain.Models;

namespace Nexus.DEB.Api.GraphQL
{
    public class StandardVersionSummaryType : EntityType<StandardVersionSummary>
    {
        public StandardVersionSummaryType(IPawsService pawsService)
        {
        }

        protected override void Configure(IObjectTypeDescriptor<StandardVersionSummary> descriptor)
        {
            descriptor.Field(x => x.StandardId).Ignore();
            descriptor.Field(x => x.StatusId).Ignore();
            descriptor.Field(x => x.EntityTypeTitle).Ignore();

			descriptor.Field("canUpVersion")
                .ResolveWith<CanUpVersionStandardVersionResolver>(i => i.GetCanUpVersionAsync(default, default, default, default, default, default));

			base.Configure(descriptor);
		}
    }
}
