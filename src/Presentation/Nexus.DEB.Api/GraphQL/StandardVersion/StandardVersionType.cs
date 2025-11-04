using Nexus.DEB.Domain.Models;

namespace Nexus.DEB.Api.GraphQL
{
    public class StandardVersionType : ObjectType<StandardVersion>
    {
        protected override void Configure(IObjectTypeDescriptor<StandardVersion> descriptor)
        {
            descriptor.Field(x => x.CreatedById).Ignore();
            descriptor.Field(x => x.LastModifiedById).Ignore();
            descriptor.Field(x => x.OwnedByGroupId).Ignore();
            descriptor.Field(x => x.OwnedById).Ignore();
            descriptor.Field(x => x.ModuleId).Ignore();
            descriptor.Field(x => x.Requirements).Ignore();
            descriptor.Field(x => x.StandardId).Ignore();

            base.Configure(descriptor);
        }
    }
}
