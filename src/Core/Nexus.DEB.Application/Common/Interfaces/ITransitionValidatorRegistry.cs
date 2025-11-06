namespace Nexus.DEB.Application.Common.Interfaces
{
    public interface ITransitionValidatorRegistry
    {
        ITransitionValidator? GetValidator(string name);
        IEnumerable<ITransitionValidator> GetValidators(IEnumerable<string> names);
    }
}
