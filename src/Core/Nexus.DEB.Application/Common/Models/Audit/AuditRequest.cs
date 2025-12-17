using System.Text.Json;

namespace Nexus.DEB.Application.Common.Models
{
    public class AuditRequest
    {
        public string EventType { get; set; } = string.Empty;
        public string PlatformTeam { get; set; } = string.Empty;
        public string ApplicationName { get; set; } = string.Empty;
        public string ApplicationInstance { get; set; } = string.Empty;
        public string EventContext { get; set; } = string.Empty;
        public Guid? UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public Guid? PostId { get; set; }
        public string PostName { get; set; } = string.Empty;
        public JsonElement? EntityId { get; set; }
        public string EntityType { get; set; } = string.Empty;
        public JsonElement? Data { get; set; }
        public string? DataTypeName { get; set; }
        public string? EnvironmentName { get; set; }
    }
}
