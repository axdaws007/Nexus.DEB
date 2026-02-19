using Nexus.DEB.Application.Common.Models;

namespace Nexus.DEB.Application.Common.Interfaces
{
    public interface ISectionDomainService
    {
        Task<Result> MoveSectionAsync(Guid sectionId, Guid? parentSectionId, int ordinal, CancellationToken cancellationToken);
    }
}
