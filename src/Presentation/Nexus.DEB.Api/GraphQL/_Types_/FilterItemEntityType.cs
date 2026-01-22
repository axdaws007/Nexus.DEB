using Nexus.DEB.Api.GraphQL._Resolvers_;
using Nexus.DEB.Application.Common.Models;
using Nexus.DEB.Application.Common.Models.Filters;

namespace Nexus.DEB.Api.GraphQL._Types_
{
	public class FilterItemEntityType : ObjectType<FilterItemEntity>
	{
		protected override void Configure(IObjectTypeDescriptor<FilterItemEntity> descriptor)
		{
			descriptor.Field("totalRequirements")
				.ResolveWith<FilterItemEntityTypeResolver>(i => i.GetTotalRequirementsAsync(default, default, default));
			descriptor.Field("status")
				.ResolveWith<FilterItemEntityTypeResolver>(i => i.GetStatus(default, default, default));
		}
	}
}
