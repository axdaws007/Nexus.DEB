using Microsoft.Extensions.Configuration;
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
            IConfiguration configuration) : base(cisService, cbacService, configuration, currentUserService, dateTimeProvider, debService)
        {
        }

        public async Task<Result<Statement>> ValidateNewStatementAsync(
            Guid ownerId,
            string title,
            string statementText,
            DateTime? reviewDate,
            ICollection<RequirementScopePair>? RequirementScopeCombinations,
            CancellationToken cancellationToken)
        {
            // Validate ownerId
            await ValidateOwnerAsync(ownerId);

            // Validate title
            ValidateTitle(title);

            // Validate statement text
            ValidateStatementText(statementText);

            // Validate review date
            ValidateReviewDate(reviewDate);

            if (validationErrors.Count > 0)
            {
                return Result<Statement>.Failure(validationErrors);
            }

            var statement = new Statement()
            {
                CreatedById = currentUserService.PostId,
                CreatedDate = dateTimeProvider.Now,
                EntityTypeTitle = EntityTypes.SoC,
                LastModifiedById = currentUserService.PostId,
                LastModifiedDate = dateTimeProvider.Now,
                ModuleId = this.moduleId,
                OwnedById = ownerId,
                ReviewDate = reviewDate,
                SerialNumber = await debService.GenerateSerialNumberAsync(moduleId, Guid.Parse("00000000-0000-0000-0000-000000000001"), EntityTypes.SoC),
                StatementText = statementText,
                Title = title
            };

            return Result<Statement>.Success(statement);
        }

        public async Task<Result<Statement>> ValidateExistingStatementAsync(
            Guid id,
            Guid ownerId,
            string title,
            string statementText,
            DateTime? reviewDate,
            ICollection<RequirementScopePair>? requirementScopeCombinations,
            CancellationToken cancellationToken)
        {
            var statement = await debService.GetStatementByIdAsync(id);

            if (statement == null)
            {
                return Result<Statement>.Failure(new ValidationError()
                {
                    Code = "INVALID_STATEMENT_ID",
                    Field = nameof(id),
                    Message = "Statement does not exist"
                });
            }

            await ValidateOwnerAsync(ownerId);

            // Validate title
            ValidateTitle(title);

            // Validate statement text
            ValidateStatementText(statementText);

            // Validate review date
            ValidateReviewDate(reviewDate);

            if (validationErrors.Count > 0)
            {
                return Result<Statement>.Failure(validationErrors);
            }

            statement.OwnedById = ownerId;
            statement.Title = title;
            statement.StatementText = statementText;
            statement.ReviewDate = reviewDate;

            return Result<Statement>.Success(statement);
        }

        private async Task ValidateOwnerAsync(Guid ownerId)
        {
            var posts = await cisService.GetAllPosts();

            if (posts != null && posts.FirstOrDefault(x => x.ID == ownerId) == null)
            {
                validationErrors.Add(
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
                validationErrors.Add(
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
                validationErrors.Add(
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
                if (reviewDate.Value < dateTimeProvider.Now)
                {
                    validationErrors.Add(
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
