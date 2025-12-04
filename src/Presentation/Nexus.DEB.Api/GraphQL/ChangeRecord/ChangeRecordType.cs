using Nexus.DEB.Domain.Models;

namespace Nexus.DEB.Api.GraphQL
{
	public class ChangeRecordType : ObjectType<ChangeRecord>
	{
		protected override void Configure(IObjectTypeDescriptor<ChangeRecord> descriptor)
		{
			descriptor.Field(x => x.ChangeRecordItems).Ignore();
			descriptor.Field(x => x.EventId).Ignore();
			descriptor.Field(x => x.IsDeleted).Ignore();

			base.Configure(descriptor);
		}
	}
}
