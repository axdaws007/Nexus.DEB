using Microsoft.Extensions.Logging;
using Nexus.DEB.Application.Common.Interfaces;
using Nexus.DEB.Application.Common.Models;

namespace Nexus.DEB.Infrastructure.Validators
{
    public class ValidateReviewDateTransitionValidator : ITransitionValidator
    {
        private readonly IPawsService _pawsService;
        private readonly IDebService _debService;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly ILogger<ValidateReviewDateTransitionValidator> _logger;

        // This name must match what's in the PAWS configuration
        public string Name => "ValidateReviewDate";

        public ValidateReviewDateTransitionValidator(
            IPawsService pawsService,
            IDebService debService,
            IDateTimeProvider dateTimeProvider,
            ILogger<ValidateReviewDateTransitionValidator> logger)
        {
            _pawsService = pawsService;
            _debService = debService;
            _dateTimeProvider = dateTimeProvider;
            _logger = logger;
        }

        public async Task<Result> ValidateAsync(
            TransitionValidationContext context,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                "Validating review date for {EntityId}",
                context.EntityId);

            var statement = await _debService.GetStatementDetailByIdAsync(context.EntityId, cancellationToken);

            if (statement.ReviewDate.HasValue == false || statement.ReviewDate.Value < _dateTimeProvider.Now.AddDays(7))
            {
                return Result.Failure(new ValidationError
                {
                    Field = "reviewDate",
                    Message = $"The Review Date must be at least one week into the future.",
                    Code = "INVALID_REVIEW_DATE",
                    Meta = new Dictionary<string, object>
                    {
                        ["reviewDate"] = statement.ReviewDate.HasValue ? statement.ReviewDate.Value : "<null>"
                    }
                });
            }

            return Result.Success();
        }
    }
}
