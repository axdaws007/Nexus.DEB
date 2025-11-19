using Nexus.DEB.Application.Common.Models;
using Nexus.DEB.Domain.Models;

namespace Nexus.DEB.Application.Common.Interfaces
{
    public interface IStatementDomainService
    {
        Task<Result<Statement>> ValidateNewStatementAsync(
            Guid ownerId,
            string title,
            string statementText,
            DateTime? reviewDate,
            ICollection<RequirementScopePair>? RequirementScopeCombinations,
            CancellationToken cancellationToken);

        Task<Result<Statement>> ValidateExistingStatementAsync(
            Guid id,
            Guid ownerId,
            string title,
            string statementText,
            DateTime? reviewDate,
            ICollection<RequirementScopePair>? requirementScopeCombinations,
            CancellationToken cancellationToken);

    }
}
