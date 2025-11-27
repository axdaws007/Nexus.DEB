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
            IPawsService pawsService) : base(cisService, cbacService, applicationSettingsService, currentUserService, dateTimeProvider, debService, pawsService, EntityTypes.SoC)
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
            await ValidateFieldsAsync(ownerId, title, statementText, reviewDate, requirementScopeCombinations);

            if (ValidationErrors.Count > 0)
            {
                return Result<Statement>.Failure(ValidationErrors);
            }

            try
            {
                var statement = new Statement()
                {
                    CreatedById = CurrentUserService.PostId,
                    CreatedDate = DateTimeProvider.Now,
                    EntityTypeTitle = EntityTypes.SoC,
                    LastModifiedById = CurrentUserService.PostId,
                    LastModifiedDate = DateTimeProvider.Now,
                    ModuleId = this.ModuleId,
                    OwnedById = ownerId,
                    ReviewDate = reviewDate,
                    SerialNumber = await DebService.GenerateSerialNumberAsync(this.ModuleId, this.InstanceId, EntityTypes.SoC),
                    Description = statementText,
                    Title = title
                };

                statement = await this.DebService.CreateStatementAsync(statement, requirementScopeCombinations, cancellationToken);

                await this.PawsService.CreateWorkflowInstanceAsync(this.WorkflowId.Value, statement.EntityId, cancellationToken);


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

            await ValidateFieldsAsync(ownerId, title, statementText, reviewDate, requirementScopeCombinations);

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

                return Result<Statement>.Success(statement);
            }
            catch(Exception ex)
            {
                return Result<Statement>.Failure($"An error occurred updating the Statement: {ex.Message}");
            }
        }

        private async Task ValidateFieldsAsync(
            Guid ownerId,
            string title,
            string statementText,
            DateTime? reviewDate,
            ICollection<RequirementScopes>? RequirementScopeCombinations)
        {
            await ValidateOwnerAsync(ownerId);

            // Validate title
            ValidateTitle(title);

            // Validate statement text
            ValidateStatementText(statementText);

            // Validate review date
            ValidateReviewDate(reviewDate);
        }

        private async Task ValidateOwnerAsync(Guid ownerId)
        {
            var posts = await CisService.GetAllPosts();

            if (posts != null && posts.FirstOrDefault(x => x.ID == ownerId) == null)
            {
                ValidationErrors.Add(
                    new ValidationError()
                    {
                        Code = "INVALID_OWNER",
                        Field = nameof(ownerId),
                        Message = "The 'Owner ID' provided does not exist as a valid Post."
                    });
            }
        }

        private void ValidateTitle(string title)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                ValidationErrors.Add(
                    new ValidationError()
                    {
                        Code = "INVALID_TITLE",
                        Field = nameof(title),
                        Message = "The 'title' is empty."
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
