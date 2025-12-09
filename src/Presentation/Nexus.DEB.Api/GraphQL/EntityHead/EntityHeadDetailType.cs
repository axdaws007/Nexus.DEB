using Nexus.DEB.Domain.Models;

namespace Nexus.DEB.Api.GraphQL.EntityHead
{
    public class EntityHeadDetailType : EntityType<EntityHeadDetail>
    {
        protected override void Configure(IObjectTypeDescriptor<EntityHeadDetail> descriptor)
        {
            base.Configure(descriptor);
        }
    }
}
