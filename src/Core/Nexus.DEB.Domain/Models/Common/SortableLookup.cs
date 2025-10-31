namespace Nexus.DEB.Domain.Models.Common
{
    public abstract class SortableLookup<T> : Lookup<T>
    {
        protected SortableLookup() : base() { }

        public int Ordinal { get; set; }
    }
}
