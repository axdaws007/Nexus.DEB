namespace Nexus.DEB.Domain.Models
{
    public class ViewPost
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public int PostTypeId { get; set; }
        public bool IsDeleted { get; set; }

    }
}
