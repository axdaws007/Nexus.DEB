using Nexus.DEB.Domain.Interfaces;

namespace Nexus.DEB.Domain.Models.Common
{
    public abstract class Lookup
    {
        protected Lookup() { }

        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsEnabled { get; set; } = true;
        public DateTime CreatedDate { get; set; }
        public DateTime LastModifiedDate { get; set; }
    }

    public abstract class Lookup<T> : Lookup, ILookup<T>
    {
        public T Id { get; set; }
    }
}