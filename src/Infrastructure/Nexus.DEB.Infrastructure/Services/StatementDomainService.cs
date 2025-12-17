using Nexus.DEB.Application.Common.Extensions;
using Nexus.DEB.Application.Common.Interfaces;
using Nexus.DEB.Application.Common.Models;
using Nexus.DEB.Domain.Models;
using Nexus.DEB.Domain.Models.Common;
using Task = System.Threading.Tasks.Task;

namespace Nexus.DEB.Infrastructure.Services
{
    public class StatementDomainService : DomainServiceBase, IStatementDomainService
    {
        public StatementDomainService(
            ICisService cisService,
            ICbacService cbacService,
            IDebService debService,
            ICurrentUserService currentUserService,
            IDateTimeProvider dateTimeProvider,
            IApplicationSettingsService applicationSettingsService,
            IPawsService pawsService,
            IAuditService auditService) : base(cisService, cbacService, applicationSettingsService, currentUserService, dateTimeProvider, debService, pawsService, auditService, EntityTypes.SoC)
        {
        }

        public async Task<Result<Statement>> CreateStatementAsync(
            Guid ownerId,
            string title,
            string statementText,
            DateTime? reviewDate,
            ICollection<RequirementScopes>? requirementScopeCombinations,
            CancellationToken cancellationToken)
        {
            await ValidateFieldsAsync(null, ownerId, title, statementText, reviewDate, requirementScopeCombinations);

            if (ValidationErrors.Count > 0)
            {
                return Result<Statement>.Failure(ValidationErrors);
            }

            try
            {
                var statement = new Statement()
                {
                    EntityTypeTitle = EntityTypes.SoC,
                    OwnedById = ownerId,
                    ReviewDate = reviewDate,
                    SerialNumber = await DebService.GenerateSerialNumberAsync(this.ModuleId, this.InstanceId, EntityTypes.SoC),
                    Description = statementText,
                    Title = title
                };

                statement = await this.DebService.CreateStatementAsync(statement, requirementScopeCombinations, cancellationToken);

                await this.PawsService.CreateWorkflowInstanceAsync(this.WorkflowId.Value, statement.EntityId, null, null, cancellationToken);

                var statementDetail = await this.DebService.GetStatementDetailByIdAsync(statement.EntityId, cancellationToken);

                await this.AuditService.EntitySaved(
                    statementDetail.EntityId,
                    EntityTypes.SoC,
                    $"Statement {statementDetail.SerialNumber} created.",
                    await this.CurrentUserService.GetUserDetailsAsync(),
                    statementDetail.ToAuditData());

                return Result<Statement>.Success(statement);
            }
            catch (Exception ex)
            {
                return Result<Statement>.Failure($"An error occurred creating the Statement: {ex.Message}");
            }
        }

        public async Task<Result<Statement>> UpdateStatementAsync(
            Guid id,
            Guid ownerId,
            string title,
            string statementText,
            DateTime? reviewDate,
            ICollection<RequirementScopes>? requirementScopeCombinations,
            CancellationToken cancellationToken)
        {
            var statement = await DebService.GetStatementByIdAsync(id);

            if (statement == null)
            {
                return Result<Statement>.Failure(new ValidationError()
                {
                    Code = "INVALID_STATEMENT_ID",
                    Field = nameof(id),
                    Message = "Statement does not exist"
                });
            }

            await ValidateFieldsAsync(statement, ownerId, title, statementText, reviewDate, requirementScopeCombinations);

            if (ValidationErrors.Count > 0)
            {
                return Result<Statement>.Failure(ValidationErrors);
            }

            statement.OwnedById = ownerId;
            statement.Title = title;
            statement.Description = statementText;
            statement.ReviewDate = reviewDate;

            try
            {
                await this.DebService.UpdateStatementAsync(statement, requirementScopeCombinations, cancellationToken);

                var statementDetail = await this.DebService.GetStatementDetailByIdAsync(statement.EntityId, cancellationToken);

                await this.AuditService.EntitySaved(
                    statementDetail.EntityId,
                    EntityTypes.SoC,
                    $"Statement {statementDetail.SerialNumber} updated.",
                    await this.CurrentUserService.GetUserDetailsAsync(),
                    statementDetail.ToAuditData());

                return Result<Statement>.Success(statement);
            }
            catch(Exception ex)
            {
                return Result<Statement>.Failure($"An error occurred updating the Statement: {ex.Message}");
            }
        }

        private async Task ValidateFieldsAsync(
            Statement? statement,
            Guid ownerId,
            string title,
            string statementText,
            DateTime? reviewDate,
            ICollection<RequirementScopes>? requirementScopeCombinations)
        {
            await ValidateOwnerAsync(ownerId);

            // Validate title
            ValidateTitle(title);

            // Validate statement text
            ValidateStatementText(statementText);

            // Validate review date
            ValidateReviewDate(reviewDate);

            await ValidateRequirementScopeCombinations(statement, requirementScopeCombinations);
        }

        private async Task ValidateRequirementScopeCombinations(
            Statement? statement,
            ICollection<RequirementScopes>? requirementScopeCombinations,
            CancellationToken cancellationToken = default)
        {
            if (requirementScopeCombinations is null || requirementScopeCombinations.Count == 0)
                return;

            // Build list of all combinations to check in one go
            var combinationsToCheck = requirementScopeCombinations
                .SelectMany(r => r.ScopeIds.Select(scopeId => (r.RequirementId, ScopeId: scopeId)))
                .ToList();

            // Batch fetch all existing combinations
            var existingCombinations = await this.DebService.GetRequirementScopeCombinations(combinationsToCheck, cancellationToken);

            // Filter to those used by OTHER statements
            var conflictingCombinations = existingCombinations
                .Where(c => statement is null || c.StatementId != statement.EntityId)
                .ToList();

            if (conflictingCombinations.Count == 0)
                return;

            // Batch fetch all entity heads we need
            var entityIds = conflictingCombinations
                .SelectMany(c => new[] { c.RequirementId, c.ScopeId, c.StatementId })
                .Distinct()
                .ToList();

            var entityHeads = await this.DebService.GetEntityHeadsAsync(entityIds, cancellationToken);

            // Build validation errors
            foreach (var conflict in conflictingCombinations)
            {
                var requirement = entityHeads.GetValueOrDefault(conflict.RequirementId);
                var scope = entityHeads.GetValueOrDefault(conflict.ScopeId);
                var usedStatement = entityHeads.GetValueOrDefault(conflict.StatementId);

                if (requirement is null || scope is null || usedStatement is null)
                    continue;

                var requirementIdentifier = $"{requirement.SerialNumber} {requirement.Title}".Trim();

                ValidationErrors.Add(new ValidationError
                {
                    Code = "INVALID_REQUIREMENT_SCOPE",
                    Field = "Requirement/Scope",
                    Message = $"The combination of requirement '{requirementIdentifier}' and scope '{scope.Title}' is already in use on Statement '{usedStatement.SerialNumber}'.",
                    Meta = new Dictionary<string, object>
                    {
                        ["requirementId"] = conflict.RequirementId,
                        ["scopeId"] = conflict.ScopeId,
                        ["conflictingStatementId"] = conflict.StatementId
                    }
                });
            }
        }

        private void ValidateStatementText(string statementText)
        {
            if (string.IsNullOrWhiteSpace(statementText))
            {
                ValidationErrors.Add(
                    new ValidationError()
                    {
                        Code = "INVALID_STATEMENT_TEXT",
                        Field = nameof(statementText),
                        Message = "The 'statement text' is empty."
                    });
            }
        }

        private void ValidateReviewDate(DateTime? reviewDate)
        {
            if (reviewDate.HasValue)
            {
                if (reviewDate.Value < DateTimeProvider.Now)
                {
                    ValidationErrors.Add(
                        new ValidationError()
                        {
                            Code = "INVALID_REVIEW_DATE",
                            Field = nameof(reviewDate),
                            Message = "A 'review date' cannot be in the past."
                        });
                }
            }
        }
    }
}
