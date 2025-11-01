namespace Nexus.DEB.Domain.Interfaces
{
    public interface IEntityHead : IEntity
    {
        public Guid CreatedById { get; set; }
        public Guid LastModifiedById { get; set; }
        public Guid OwnedById { get; set; }
    }
}
