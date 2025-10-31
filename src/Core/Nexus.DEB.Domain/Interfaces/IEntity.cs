namespace Nexus.DEB.Domain.Interfaces
{
    public interface IEntity
    {
        public Guid Id { get; set; }
        public Guid CreatedById { get; set; }
        public Guid LastModifiedById { get; set; }
    }
}
