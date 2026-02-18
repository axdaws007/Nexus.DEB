namespace Nexus.DEB.Domain.Interfaces
{
    public interface IEntityHead : IEntity, IOwnedBy, IEntityType
    {
        public Guid CreatedById { get; set; }
        public Guid LastModifiedById { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime LastModifiedDate { get; set; }
        public Guid ModuleId { get; set; }
    }
}
