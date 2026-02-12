using Nexus.DEB.Application.Common.Models;
using Nexus.DEB.Application.Common.Models.Filters;

namespace Nexus.DEB.Api.GraphQL
{
	public class StandardVersionDetailType : EntityType<StandardVersionDetail>
	{
		protected override void Configure(IObjectTypeDescriptor<StandardVersionDetail> descriptor)
		{
			base.Configure(descriptor);
		}
	}
}
