using HotChocolate.Authorization;
using Nexus.DEB.Application.Common.Interfaces;
using Nexus.DEB.Application.Common.Models;
using Nexus.DEB.Domain.Models;
using Nexus.DEB.Domain.Models.Common;

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
            ICollection<RequirementScopePair>? requirementScopeCombinations,
            IStatementDomainService statementService,
            IDebService debService,
            IPawsService pawsService,
            IApplicationSettingsService applicationSettingsService,
            CancellationToken cancellationToken = default)
        {
            var moduleId = applicationSettingsService.GetModuleId("DEB");
            var workflowId = await debService.GetWorkflowIdAsync(moduleId, EntityTypes.SoC, cancellationToken);

            var result = await statementService.ValidateNewStatementAsync(
                ownerId,
                title,
                statementText,
                reviewDate,
                requirementScopeCombinations,
                cancellationToken);

            if (!result.IsSuccess)
            {
                throw BuildException(result);
            }

            var statement = await debService.CreateStatementAsync(result.Data, cancellationToken);

            await pawsService.CreateWorkflowInstanceAsync(workflowId.Value, statement.EntityId, cancellationToken);

            return statement;
        }

        [Authorize]
        public static async Task<Statement?> UpdateStatementAsync(
            Guid id,
            Guid ownerId,
            string title,
            string statementText,
            DateTime? reviewDate,
            ICollection<RequirementScopePair>? requirementScopeCombinations,
            IStatementDomainService statementService,
            IDebService debService,
            CancellationToken cancellationToken = default)
        {

            var result = await statementService.ValidateExistingStatementAsync(
                id,
                ownerId,
                title,
                statementText,
                reviewDate,
                requirementScopeCombinations,
                cancellationToken);

            if (!result.IsSuccess)
            {
                throw BuildException(result);
            }

            return await debService.UpdateStatementAsync(result.Data, cancellationToken);
        }

        private static GraphQLException BuildException(Result result)
        {
            var errors = result.Errors.Select(e =>
                ErrorBuilder.New()
                    .SetMessage(e.Message)
                    .SetCode(e.Code)
                    .SetExtension("field", e.Field)
                    .SetExtension("meta", e.Meta)
                    .Build());

            return new GraphQLException(errors);
        }
    }
}
