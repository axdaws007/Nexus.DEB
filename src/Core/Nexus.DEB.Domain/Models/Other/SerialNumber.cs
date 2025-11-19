namespace Nexus.DEB.Domain.Models
{
    public class SerialNumber
    {
        public Guid SerialNumberId { get; set; }
        public Guid ModuleId { get; set; }
        public Guid InstanceId { get; set; }
        public string EntityType { get; set; } = string.Empty;
        public int NextValue { get; set; }
        public string Format { get; set; } = string.Empty;
    }
}
