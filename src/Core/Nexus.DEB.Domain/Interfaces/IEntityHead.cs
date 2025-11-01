namespace Nexus.DEB.Domain.Interfaces
{
    public interface IEntityHead : IEntity, IOwnedBy
    {
        public Guid CreatedById { get; set; }
        public Guid LastModifiedById { get; set; }
    }
}
