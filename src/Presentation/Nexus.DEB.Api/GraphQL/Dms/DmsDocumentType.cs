using Nexus.DEB.Application.Common.Models.Dms;

namespace Nexus.DEB.Api.GraphQL.Dms
{
    public class DmsDocumentType : ObjectType<DmsDocument>
    {
        protected override void Configure(IObjectTypeDescriptor<DmsDocument> descriptor)
        {
            descriptor.Field(x => x.DocumentOwnerId).Ignore();

            base.Configure(descriptor);
        }
    }
}
