namespace Nexus.DEB.Domain.Models
{
    public class MyWorkDetailItem
    {
        public Guid EntityID { get; set; }

        public Guid ModuleID { get; set; }

        public string EntityTypeTitle { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;

        public string SerialNumber { get; set; } = string.Empty;

        public DateTime CreatedDate { get; set; }

        public DateTime ModifiedDate { get; set; }

        public DateTime? DueDate { get; set; }

        public DateTime? ReviewDate { get; set; }

        public string PendingActivityList { get; set; } = string.Empty;

        public string PendingActivityOwners { get; set; } = string.Empty;

        public string? OwnerGroup { get; set; }

        public string OwnerPost { get; set; } = string.Empty;

        public string TransferDates { get; set; } = string.Empty;
    }
}
