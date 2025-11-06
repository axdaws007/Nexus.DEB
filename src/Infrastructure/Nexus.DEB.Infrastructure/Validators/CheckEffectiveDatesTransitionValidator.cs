using Microsoft.Extensions.Logging;
using Nexus.DEB.Application.Common.Interfaces;
using Nexus.DEB.Application.Common.Models;

namespace Nexus.DEB.Infrastructure.Validators
{
    public class CheckEffectiveDatesTransitionValidator : ITransitionValidator
    {
        private readonly IPawsService _pawsService;
        private readonly IDebService _debService;
        private readonly ILogger<CheckEffectiveDatesTransitionValidator> _logger;

        // This name must match what's in the PAWS configuration
        public string Name => "CheckEffectiveDates";

        public CheckEffectiveDatesTransitionValidator(
            IPawsService pawsService,
            IDebService debService,
            ILogger<CheckEffectiveDatesTransitionValidator> logger)
        {
            _pawsService = pawsService;
            _debService = debService;
            _logger = logger;
        }

        public async Task<Result> ValidateAsync(
            TransitionValidationContext context,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                "Validating effective dates for {EntityId}",
                context.EntityId);

            var standardVersion = await _debService.GetStandardVersionByIdAsync(context.EntityId, cancellationToken);

            if (standardVersion.EffectiveEndDate.HasValue && standardVersion.EffectiveEndDate < standardVersion.EffectiveStartDate)
            {
                return Result.Failure(new ValidationError
                {
                    Field = "effectiveEndDate",
                    Message = $"The Effective End Date cannot be earlier than the Effective Start Date",
                    Code = "INVALID_EFFECTIVE_DATES",
                    Meta = new Dictionary<string, object>
                    {
                        ["effectiveStartDate"] = standardVersion.EffectiveStartDate,
                        ["effectiveEndDate"] = standardVersion.EffectiveEndDate
                    }
                });
            }

            return Result.Success();
        }
    }
}
