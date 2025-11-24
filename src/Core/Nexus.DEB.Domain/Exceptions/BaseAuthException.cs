namespace Nexus.DEB.Domain.Exceptions
{
    public abstract class BaseAuthException(string userName, Guid userId, string entityType, string id, string authorisationType)
        : Exception($"AUTH ERROR : User '{userName}' ({userId}) does not have the appropriate {authorisationType}.")
    {
        public string UserName { get; init; } = userName;
        public Guid UserId { get; init; } = userId;
        public string EntityType { get; init; } = entityType;
        public string Id { get; init; } = id;
    }
}
