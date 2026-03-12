using Nexus.DEB.Application.Common.Models;
using Nexus.DEB.Application.Common.Models.Filters;

namespace Nexus.DEB.Api.GraphQL
{
	public class StandardVersionDetailType : EntityType<StandardVersionDetail>
	{
		protected override void Configure(IObjectTypeDescriptor<StandardVersionDetail> descriptor)
		{
			descriptor.Field("canUpVersion")
				.ResolveWith<CanUpVersionStandardVersionResolver>(i => i.GetCanUpVersionAsync(default, default, default, default, default, default));

			base.Configure(descriptor);
		}
	}
}
