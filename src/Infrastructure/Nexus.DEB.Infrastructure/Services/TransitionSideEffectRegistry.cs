using Microsoft.Extensions.Logging;
using Nexus.DEB.Application.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nexus.DEB.Infrastructure.Services
{
    public class TransitionSideEffectRegistry : ITransitionSideEffectRegistry
    {
        private readonly Dictionary<string, ITransitionSideEffect> _sideEffects;
        private readonly ILogger<TransitionSideEffectRegistry> _logger;

        public TransitionSideEffectRegistry(Dictionary<string, ITransitionSideEffect> sideEffects, ILogger<TransitionSideEffectRegistry> logger)
        {
            _sideEffects = sideEffects;
            _logger = logger;
        }

        public ITransitionSideEffect? GetSideEffect(string name)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ITransitionSideEffect> GetSideEffects(IEnumerable<string> names)
        {
            throw new NotImplementedException();
        }
    }
}
