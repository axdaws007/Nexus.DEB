using Nexus.DEB.Application.Common.Models;

namespace Nexus.DEB.Api.GraphQL
{
    public static class ExceptionHelper
    {
        public static GraphQLException BuildException(Result result)
        {
            var errors = result.Errors.Select(e =>
                ErrorBuilder.New()
                    .SetMessage(e.Message)
                    .SetCode(e.Code)
                    .SetExtension("field", e.Field)
                    .SetExtension("meta", e.Meta)
                    .Build());

            return new GraphQLException(errors);
        }

        public static GraphQLException BuildException(Exception exception)
        {
            return new GraphQLException("An error has occurred.", exception);
        }
    }
}
