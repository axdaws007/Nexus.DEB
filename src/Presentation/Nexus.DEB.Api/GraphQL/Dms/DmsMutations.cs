using HotChocolate.Authorization;
using Nexus.DEB.Application.Common.Interfaces;
using Nexus.DEB.Domain;

namespace Nexus.DEB.Api.GraphQL
{
    [MutationType]
    public static class DmsMutations
    {
        [Authorize]
        [UseMutationConvention(Disable = true)]
        public static async Task<bool> DeleteDocument(
           string library,
           Guid documentId,
           IApplicationSettingsService applicationSettingsService,
           IDmsService dmsService)
        {
            try
            {
                DebHelper.Dms.Libraries.ValidateOrThrow(library);
            }
            catch (Exception ex)
            {
                throw ExceptionHelper.BuildException(ex);
            }

            var libraryId = applicationSettingsService.GetLibraryId(library);

            return await dmsService.DeleteDocumentAsync(libraryId, documentId);
        }
    }
}
