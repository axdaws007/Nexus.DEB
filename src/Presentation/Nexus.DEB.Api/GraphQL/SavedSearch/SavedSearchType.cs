using Nexus.DEB.Application.Common.Interfaces;
using Nexus.DEB.Domain.Models;

namespace Nexus.DEB.Api.GraphQL
{
	public class SavedSearchType : ObjectType<SavedSearch>
	{
		protected override void Configure(IObjectTypeDescriptor<SavedSearch> descriptor)
		{
			descriptor.Field(x => x.PostId).Ignore();

			base.Configure(descriptor);
		}
	}
}
