namespace Nexus.DEB.Domain.Exceptions
{
    public class CapabilityException(string userName, Guid userId, string entityType, string id)
        : BaseAuthException(userName, userId, entityType, id, "capability")
    {
    }
}
