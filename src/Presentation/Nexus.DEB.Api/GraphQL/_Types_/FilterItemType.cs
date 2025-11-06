using Nexus.DEB.Application.Common.Models.Filters;

namespace Nexus.DEB.Api.GraphQL._Types_
{
    public class FilterItemType : ObjectType<FilterItem>
    {
        protected override void Configure(IObjectTypeDescriptor<FilterItem> descriptor)
        {
            descriptor.Field(x => x.Id).ID();

            base.Configure(descriptor);
        }
    }
}
