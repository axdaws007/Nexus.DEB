using Nexus.DEB.Domain.Models;

namespace Nexus.DEB.Api.GraphQL
{
    public class RequirementTypeType : ObjectType<RequirementType>
    {
        protected override void Configure(IObjectTypeDescriptor<RequirementType> descriptor)
        {
            descriptor.Field(x => x.IsEnabled).Ignore();

            base.Configure(descriptor);
        }
    }
}
