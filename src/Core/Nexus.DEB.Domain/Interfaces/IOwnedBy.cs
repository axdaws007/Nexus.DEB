namespace Nexus.DEB.Domain.Interfaces
{
    public interface IOwnedBy
    {
        public Guid OwnedById { get; set; }
    }
}
