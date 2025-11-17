using Nexus.DEB.Application.Common.Models;

namespace Nexus.DEB.Api.GraphQL.Paws
{
    public class TargetActivityType : ObjectType<TargetActivity>
    {
        protected override void Configure(IObjectTypeDescriptor<TargetActivity> descriptor)
        {
            descriptor.Field(x => x.SideEffectTags).Ignore();
            descriptor.Field(x => x.ValidatorTags).Ignore();

            base.Configure(descriptor);
        }
    }
}
