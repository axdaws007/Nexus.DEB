using Nexus.DEB.Application.Common.Models;

namespace Nexus.DEB.Application.Common.Interfaces
{
    public interface IStatementDomainService
    {
        Task<Result<StatementDetail>> CreateStatementAsync(
            Guid ownerId,
            string title,
            string statementText,
            DateOnly reviewDate,
            ICollection<RequirementScopes>? RequirementScopeCombinations,
            CancellationToken cancellationToken);

        Task<Result<StatementDetail>> UpdateStatementAsync(
            Guid id,
            Guid ownerId,
            string title,
            string statementText,
            DateOnly reviewDate,
            ICollection<RequirementScopes>? requirementScopeCombinations,
            CancellationToken cancellationToken);

    }
}
