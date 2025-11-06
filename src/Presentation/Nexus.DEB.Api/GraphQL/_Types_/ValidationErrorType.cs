using Nexus.DEB.Application.Common.Models;

namespace Nexus.DEB.Api.GraphQL
{
    public class ValidationErrorType : ObjectType<ValidationError>
    {
        protected override void Configure(IObjectTypeDescriptor<ValidationError> descriptor)
        {
            descriptor.Field(x => x.Meta).Ignore();

            base.Configure(descriptor);
        }
    }
}
