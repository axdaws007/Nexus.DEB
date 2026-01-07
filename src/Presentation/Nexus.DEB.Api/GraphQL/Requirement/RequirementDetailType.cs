using Nexus.DEB.Application.Common.Models;

namespace Nexus.DEB.Api.GraphQL.Requirement
{
	public class RequirementDetailType : EntityType<RequirementDetail>
	{
		protected override void Configure(IObjectTypeDescriptor<RequirementDetail> descriptor)
		{
			base.Configure(descriptor);
		}
	}
}
