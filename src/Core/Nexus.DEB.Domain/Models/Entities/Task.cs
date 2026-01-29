using Nexus.DEB.Domain.Models.Common;

namespace Nexus.DEB.Domain.Models
{
    public class Task : EntityHead
    {
        public short TaskTypeId { get; set; }
        public virtual TaskType TaskType { get; set; }
        public DateOnly? DueDate { get; set; }
        public virtual Statement Statement { get; set; }
        public Guid StatementId { get; set; }
        public DateOnly? OriginalDueDate { get; set; }

    }
}
