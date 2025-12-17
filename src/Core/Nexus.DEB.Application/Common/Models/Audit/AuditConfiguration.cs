namespace Nexus.DEB.Application.Common.Models
{
    public class AuditConfiguration
    {
        public string PlatformTeam { get; set; } = string.Empty;
        public string ApplicationName { get; set; } = string.Empty;
        public string ApplicationInstance { get; set; } = string.Empty;
        public string? EnvironmentName { get; set; }
    }
}
