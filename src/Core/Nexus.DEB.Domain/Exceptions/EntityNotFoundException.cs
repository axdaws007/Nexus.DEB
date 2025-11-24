namespace Nexus.DEB.Domain.Exceptions
{
    public sealed class EntityNotFoundException : Exception
    {
        public EntityNotFoundException(string entityType, int id)
            : base($"{entityType} `{id}` was not found.")
        {
            Id = id.ToString();
        }

        public EntityNotFoundException(string entityType, long id)
            : base($"{entityType} `{id}` was not found.")
        {
            Id = id.ToString();
        }

        public EntityNotFoundException(string entityType, Guid id)
            : base($"{entityType} `{id}` was not found.")
        {
            Id = id.ToString();
        }

        public string Id { get; }
    }
}
