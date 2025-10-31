using Nexus.DEB.Domain.Models;

namespace Nexus.DEB.Application.Common.Interfaces
{
    public interface IDebService
    {
        IQueryable<StandardVersionSummary> GetStandardVersionsForGrid();
    }
}
