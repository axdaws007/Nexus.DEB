namespace Nexus.DEB.Domain.Models
{
    public enum OwnerRequired
    {
        Optional = 1,
        Mandatory = 2,
        MustNotAssign = 3
    }

    public enum TransitionType
    {
        Progressive = 1,
        Regressive = 2
    }
}
