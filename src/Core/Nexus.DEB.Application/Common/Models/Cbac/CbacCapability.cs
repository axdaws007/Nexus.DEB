namespace Nexus.DEB.Application.Common.Models
{
    public class CbacCapability
    {
        public Guid ModuleID { get; set; }
        public string CapabilityName { get; set; } = string.Empty;
        public Guid CapabilityID { get; set; }
        public bool IsAllowed { get; set; }
    }
}
