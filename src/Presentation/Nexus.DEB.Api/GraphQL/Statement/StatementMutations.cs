using HotChocolate.Authorization;
using Nexus.DEB.Application.Common.Interfaces;
using Nexus.DEB.Application.Common.Models;
using Nexus.DEB.Domain.Models;

namespace Nexus.DEB.Api.GraphQL
{
    [MutationType]
    public static class StatementMutations
    {
        [Authorize]
        public static async Task<Statement?> CreateStatementAsync(
            Guid ownerId,
            string title,
            string statementText,
            DateTime? reviewDate,
            ICollection<RequirementScopes> requirementScopeCombinations,
            IStatementDomainService statementService,
            CancellationToken cancellationToken = default)
        {
            var result = await statementService.CreateStatementAsync(
                ownerId,
                title,
                statementText,
                reviewDate,
                requirementScopeCombinations,
                cancellationToken);

            if (!result.IsSuccess)
            {
                throw ExceptionHelper.BuildException(result);
            }

            return result.Data;
        }

        [Authorize]
        public static async Task<Statement?> UpdateStatementAsync(
            Guid id,
            Guid ownerId,
            string title,
            string statementText,
            DateTime? reviewDate,
            ICollection<RequirementScopes> requirementScopeCombinations,
            IStatementDomainService statementService,
            CancellationToken cancellationToken = default)
        {
            var result = await statementService.UpdateStatementAsync(
                id,
                ownerId,
                title,
                statementText,
                reviewDate,
                requirementScopeCombinations,
                cancellationToken);

            if (!result.IsSuccess)
            {
                throw ExceptionHelper.BuildException(result);
            }

            return result.Data;
        }
    }
}
