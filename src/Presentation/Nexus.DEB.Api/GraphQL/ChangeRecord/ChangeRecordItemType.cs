using Nexus.DEB.Domain.Models;

namespace Nexus.DEB.Api.GraphQL
{
	public class ChangeRecordItemType : ObjectType<ChangeRecordItem>
	{
		protected override void Configure(IObjectTypeDescriptor<ChangeRecordItem> descriptor)
		{
			descriptor.Field(x => x.ChangeRecord).Ignore();
			descriptor.Field(x => x.IsDeleted).Ignore();

			base.Configure(descriptor);
		}
	}
}
