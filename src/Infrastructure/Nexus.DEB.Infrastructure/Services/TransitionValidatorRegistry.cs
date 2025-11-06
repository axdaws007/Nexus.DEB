using Microsoft.Extensions.Logging;
using Nexus.DEB.Application.Common.Interfaces;

namespace Nexus.DEB.Infrastructure.Services
{
    public class TransitionValidatorRegistry : ITransitionValidatorRegistry
    {
        private readonly Dictionary<string, ITransitionValidator> _validators;
        private readonly ILogger<TransitionValidatorRegistry> _logger;

        public TransitionValidatorRegistry(
            IEnumerable<ITransitionValidator> validators,
            ILogger<TransitionValidatorRegistry> logger)
        {
            _logger = logger;

            // Build dictionary of validators by name
            _validators = validators.ToDictionary(
                v => v.Name,
                v => v,
                StringComparer.OrdinalIgnoreCase);

            _logger.LogInformation(
                "Registered {Count} transition validators: {Names}",
                _validators.Count,
                string.Join(", ", _validators.Keys));
        }

        public ITransitionValidator? GetValidator(string name)
        {
            _validators.TryGetValue(name, out var validator);
            return validator;
        }

        public IEnumerable<ITransitionValidator> GetValidators(IEnumerable<string> names)
        {
            var result = new List<ITransitionValidator>();

            foreach (var name in names)
            {
                var validator = GetValidator(name);
                if (validator != null)
                {
                    result.Add(validator);
                }
                else
                {
                    _logger.LogWarning(
                        "Validator '{Name}' not found in registry. Check configuration.",
                        name);
                }
            }

            return result;
        }
    }
}
