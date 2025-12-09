using Nexus.DEB.Application.Common.Models;

namespace Nexus.DEB.Api.GraphQL
{
    public class StatementSummaryType : EntityType<StatementSummary>
    {
        protected override void Configure(IObjectTypeDescriptor<StatementSummary> descriptor)
        {
            descriptor.Field(x => x.EntityTypeTitle).Ignore();
            base.Configure(descriptor);
        }
    }
}
