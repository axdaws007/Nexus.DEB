namespace Nexus.DEB.Application.Common.Models
{
    public class PostDetails
    {
        public Guid ID { get; set; }
        public string PostTitle { get; set; }
        public string? Description { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateModified { get; set; }
        public int? PostTypeID { get; set; }
        public string? PostTypeName { get; set; }
    }
}
