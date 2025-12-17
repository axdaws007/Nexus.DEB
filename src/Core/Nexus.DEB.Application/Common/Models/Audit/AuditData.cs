using System.Text.Json;

namespace Nexus.DEB.Application.Common.Models
{
    public record AuditData(JsonElement Data, string TypeName);
}
