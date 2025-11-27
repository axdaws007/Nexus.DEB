using Nexus.DEB.Application.Common.Models;
using Nexus.DEB.Domain.Models;

namespace Nexus.DEB.Application.Common.Interfaces
{
    public interface IStatementDomainService
    {
        Task<Result<Statement>> CreateStatementAsync(
            Guid ownerId,
            string title,
            string statementText,
            DateTime? reviewDate,
            ICollection<RequirementScopes>? RequirementScopeCombinations,
            CancellationToken cancellationToken);

        Task<Result<Statement>> UpdateStatementAsync(
            Guid id,
            Guid ownerId,
            string title,
            string statementText,
            DateTime? reviewDate,
            ICollection<RequirementScopes>? requirementScopeCombinations,
            CancellationToken cancellationToken);

    }
}
