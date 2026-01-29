using System.Text.Json;

namespace Nexus.DEB.Application.Common.Models.Dms
{
    public class DmsDocumentMetadata
    {
        /// <summary>
        /// The raw JSON string to pass through to the legacy API.
        /// </summary>
        public string? RawJson { get; set; }

        /// <summary>
        /// Parsed fields for inspection (values can be any JSON type).
        /// </summary>
        public Dictionary<string, JsonElement> Fields { get; set; } = new();

        public bool TryGetGuid(string fieldName, out Guid value)
        {
            value = Guid.Empty;
            if (Fields.TryGetValue(fieldName, out var element) &&
                element.ValueKind == JsonValueKind.String)
            {
                return Guid.TryParse(element.GetString(), out value);
            }
            return false;
        }

        public string? GetValueOrDefault(string fieldName)
        {
            if (Fields.TryGetValue(fieldName, out var element) &&
                element.ValueKind == JsonValueKind.String)
            {
                return element.GetString();
            }
            return null;
        }
    }
}
