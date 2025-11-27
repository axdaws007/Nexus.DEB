using Nexus.DEB.Application.Common.Models;
using Nexus.DEB.Domain.Models;

namespace Nexus.DEB.Application.Common.Interfaces
{
    public interface ICommentDomainService
    {
        Task<Result<CommentDetail>> CreateCommentAsync(
            Guid entityId,
            string text,
            IDebUser debUser,
            CancellationToken cancellationToken);

        Task<Result> DeleteCommentByIdAsync(
            long id,
            IDebUser debUser,
            CancellationToken cancellationToken);
    }
}
