namespace Nexus.DEB.Domain.Models
{
    public class MyWorkSummaryItem
    {
        /// <summary>
        /// PostID
        /// </summary>
        public Guid PostId { get; set; }

        /// <summary>
        /// Post title.
        /// </summary>
        public string PostTitle { get; set; } = string.Empty;


        /// <summary>
        /// The entity type title as found in the EntityHead table.
        /// </summary>
        public string EntityTypeTitle { get; set; } = string.Empty;


        /// <summary>
        /// Number of open forms requiring attention.
        /// </summary>
        public int FormCount { get; set; }
    }
}
