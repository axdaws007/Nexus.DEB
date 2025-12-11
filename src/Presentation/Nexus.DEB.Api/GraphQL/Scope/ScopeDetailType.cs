using Nexus.DEB.Application.Common.Models;

namespace Nexus.DEB.Api.GraphQL
{
	public class ScopeDetailType : EntityType<ScopeDetail>
	{
		protected override void Configure(IObjectTypeDescriptor<ScopeDetail> descriptor)
		{
			base.Configure(descriptor);
		}
	}
}
