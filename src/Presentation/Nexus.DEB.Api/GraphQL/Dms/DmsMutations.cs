using HotChocolate.Authorization;
using Nexus.DEB.Application.Common.Interfaces;

namespace Nexus.DEB.Api.GraphQL
{
    [MutationType]
    public static class DmsMutations
    {
        [Authorize]
        public static async Task<bool> DeleteDocument(
           Guid libraryId,
           Guid documentId,
           [Service] IDmsService dmsService)
        {
            return await dmsService.DeleteDocumentAsync(libraryId, documentId);
        }
    }
}
