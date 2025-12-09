using Nexus.DEB.Domain.Interfaces;

namespace Nexus.DEB.Api.GraphQL
{
    public abstract class EntityType<T> : ObjectType<T> where T : IEntityType
    {
        protected override void Configure(IObjectTypeDescriptor<T> descriptor)
        {
            descriptor.Field("canEdit")
                .ResolveWith<EntityTypeResolver>(i => i.GetCanEditAsync(default, default, default, default));

        }
    }
}
