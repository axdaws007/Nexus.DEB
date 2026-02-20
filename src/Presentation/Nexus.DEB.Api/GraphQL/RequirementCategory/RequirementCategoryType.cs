using Nexus.DEB.Domain.Models;

namespace Nexus.DEB.Api.GraphQL
{
    public class RequirementCategoryType : ObjectType<RequirementCategory>
    {
        protected override void Configure(IObjectTypeDescriptor<RequirementCategory> descriptor)
        {
            descriptor.Field(x => x.IsEnabled).Ignore();

            base.Configure(descriptor);
        }
    }
}
