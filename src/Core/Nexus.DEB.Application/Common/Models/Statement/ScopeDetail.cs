namespace Nexus.DEB.Application.Common.Models
{
    public class ScopeDetail
    {
        public Guid ScopeId { get; set; }
        public string? SerialNumber { get; set; }
        public string Title { get; set; } = string.Empty;
        public bool IsSelected { get; set; }
    }
}
