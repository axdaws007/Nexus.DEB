using Microsoft.Extensions.Logging;
using Nexus.DEB.Application.Common.Interfaces;

namespace Nexus.DEB.Infrastructure.Services
{
    public class TransitionSideEffectRegistry : ITransitionSideEffectRegistry
    {
        private readonly Dictionary<string, ITransitionSideEffect> _sideEffects;
        private readonly ILogger<TransitionSideEffectRegistry> _logger;

        public TransitionSideEffectRegistry(
            IEnumerable<ITransitionSideEffect> sideEffects, 
            ILogger<TransitionSideEffectRegistry> logger)
        {
            _logger = logger;

            // Build dictionary of validators by name
            _sideEffects = sideEffects.ToDictionary(
                v => v.Name,
                v => v,
                StringComparer.OrdinalIgnoreCase);

            _logger.LogInformation(
                "Registered {Count} transition validators: {Names}",
                _sideEffects.Count,
                string.Join(", ", _sideEffects.Keys));
        }

        public ITransitionSideEffect? GetSideEffect(string name)
        {
            _sideEffects.TryGetValue(name, out var sideEffect);
            return sideEffect;
        }

        public IEnumerable<ITransitionSideEffect> GetSideEffects(IEnumerable<string> names)
        {
            var result = new List<ITransitionSideEffect>();

            foreach (var name in names)
            {
                var sideEffect = GetSideEffect(name);
                if (sideEffect != null)
                {
                    result.Add(sideEffect);
                }
                else
                {
                    _logger.LogWarning(
                        "Side Effect '{Name}' not found in registry. Check configuration.",
                        name);
                }
            }

            return result;
        }
    }
}
