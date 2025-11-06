namespace Nexus.DEB.Application.Common.Interfaces
{
    public interface ITransitionSideEffectRegistry
    {
        ITransitionSideEffect? GetSideEffect(string name);
        IEnumerable<ITransitionSideEffect> GetSideEffects(IEnumerable<string> names);
    }
}
