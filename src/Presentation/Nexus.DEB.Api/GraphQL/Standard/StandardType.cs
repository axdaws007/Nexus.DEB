using Nexus.DEB.Domain.Models;

namespace Nexus.DEB.Api.GraphQL
{
    public class StandardType : ObjectType<Standard>
    {
        protected override void Configure(IObjectTypeDescriptor<Standard> descriptor)
        {
            descriptor.Field(x => x.IsEnabled).Ignore();
            base.Configure(descriptor);
        }
    }
}
