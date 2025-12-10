using Nexus.DEB.Application.Common.Models;
using Nexus.DEB.Domain.Models;

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
