using Nexus.DEB.Application.Common.Models;
using Nexus.DEB.Domain.Models;

namespace Nexus.DEB.Application.Common.Interfaces
{
    public interface ISectionDomainService
    {
        Task<Result> MoveSectionAsync(Guid sectionId, Guid? parentSectionId, int ordinal, CancellationToken cancellationToken);
        Task<Result<Section>> CreateSectionAsync(string reference, string title, bool displayReference, bool displayTitle, Guid? parentId, Guid standardVersionId, CancellationToken cancellationToken);
        Task<Result<Section>> UpdateSectionAsync(Guid sectionId, string reference, string title, bool displayReference, bool displayTitle, CancellationToken cancellationToken);
        Task<Result<bool>> DeleteSectionAsync(Guid sectionId, CancellationToken cancellationToken);
    }
}
