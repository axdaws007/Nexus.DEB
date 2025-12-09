using Nexus.DEB.Application.Common.Models;

namespace Nexus.DEB.Api.GraphQL
{
    public class StatementDetailType : EntityType<StatementDetail>
    {
        protected override void Configure(IObjectTypeDescriptor<StatementDetail> descriptor)
        {
            base.Configure(descriptor);
        }
    }
}
