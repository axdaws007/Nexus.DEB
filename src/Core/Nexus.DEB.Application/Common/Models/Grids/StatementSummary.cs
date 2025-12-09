using Nexus.DEB.Domain.Interfaces;

namespace Nexus.DEB.Application.Common.Models
{
    public class StatementSummary : IEntityType, IOwnedBy
    {
        public Guid EntityId { get; set; }
        public string SerialNumber { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public DateTime LastModifiedDate { get; set; }
        public Guid OwnedById { get; set; }
        public string? OwnedBy { get; set; }
        public ICollection<ChildItem> Requirements { get; set; } = new List<ChildItem>();
        public int? StatusId { get; set; }
        public string? Status { get; set; }
        public string EntityTypeTitle { get; set; } = string.Empty;
    }
}
