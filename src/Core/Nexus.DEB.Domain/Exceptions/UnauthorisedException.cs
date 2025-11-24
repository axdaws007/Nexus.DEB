namespace Nexus.DEB.Domain.Exceptions
{
    public class UnauthorisedException(string userName, Guid userId, string entityType, string id)
        : BaseAuthException(userName, userId, entityType, id, "authorisation")
    {
    }
}
