using Microsoft.Extensions.Logging;
using Nexus.DEB.Application.Common.Interfaces;
using Nexus.DEB.Application.Common.Models;
using Nexus.DEB.Domain;

namespace Nexus.DEB.Infrastructure.Validators
{
    public class ActiveStandardVersionsValidator : ITransitionValidator
    {
        private readonly IDebService _debService;
        private readonly ILogger<ActiveStandardVersionsValidator> _logger;

        public string Name => "ValidateActiveStandardVersions";

        public ActiveStandardVersionsValidator(IDebService debService, ILogger<ActiveStandardVersionsValidator> logger)
        {
            _debService = debService;
            _logger = logger;
        }

        public async Task<Result> ValidateAsync(TransitionValidationContext context, CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Validating any other active Standard Versions for {EntityId}", context.EntityId);

            var standardVersion = await _debService.GetStandardVersionByIdAsync(context.EntityId, cancellationToken);

            var activeStandardVersions = await _debService.GetStandardVersionsForThisStandardAndStatusAsync(standardVersion.StandardId, DebHelper.Paws.States.Active, cancellationToken);

            if (activeStandardVersions.Count > 0)
            {
                var standardVersionTitles = string.Join(", ", activeStandardVersions.Select(x => x.Title));

                _logger.LogDebug("Validation failed.");

                return Result.Failure(new ValidationError
                {
                    Field = "Status",
                    Message = $"There is an active Standard Version '{standardVersionTitles}'. This Standard Version cannot be moved to 'active' whilst another Standard Version for this Standard is 'active'.",
                    Code = "ACTIVE_STANDARD_VERSION_EXISTS"
                });
            }

            _logger.LogDebug("Validation successful.");

            return Result.Success();
        }
    }
}
