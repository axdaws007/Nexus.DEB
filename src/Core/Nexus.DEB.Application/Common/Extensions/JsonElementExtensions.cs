using Nexus.DEB.Application.Common.Models;
using System.Collections;
using System.Reflection;
using System.Reflection.Metadata;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nexus.DEB.Application.Common.Extensions;

public static class JsonElementExtensions
{
    private static readonly JsonSerializerOptions DefaultOptions = new()
    {
        ReferenceHandler = ReferenceHandler.IgnoreCycles,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    /// <summary>
    /// Default maximum depth for deep serialization to prevent stack overflow on circular references.
    /// </summary>
    private const int DefaultMaxDepth = 5;

    public static JsonElement ToJsonElement(this object? value)
    {
        if (value == null)
        {
            return JsonDocument.Parse("null").RootElement.Clone();
        }

        // If it's already a JsonElement, return it
        if (value is JsonElement existingElement)
        {
            return existingElement.Clone();
        }

        var json = JsonSerializer.Serialize(value, DefaultOptions);
        using var doc = JsonDocument.Parse(json);
        return doc.RootElement.Clone();
    }

    #region Shallow Serialization (Original)

    /// <summary>
    /// Converts an object to a shallow JsonElement with type name for audit logging.
    /// Only includes scalar properties (no nested objects or collections).
    /// </summary>
    public static AuditData ToAuditData<T>(this T value)
    {
        var typeName = typeof(T).Name;

        // Handle anonymous types - extract a cleaner name
        if (typeName.StartsWith("<>"))
        {
            typeName = "AuditData";
        }

        var jsonElement = value.ToShallowJsonElement();
        return new AuditData(jsonElement, typeName);
    }

    /// <summary>
    /// Converts an object to a shallow JsonElement with a custom type name.
    /// Only includes scalar properties (no nested objects or collections).
    /// </summary>
    public static AuditData ToAuditData<T>(this T value, string typeName)
    {
        var jsonElement = value.ToShallowJsonElement();
        return new AuditData(jsonElement, typeName);
    }

    public static JsonElement ToShallowJsonElement<T>(this T value)
    {
        if (value == null)
        {
            return JsonDocument.Parse("null").RootElement.Clone();
        }
        if (IsScalarType(value.GetType()))
        {
			var json = JsonSerializer.Serialize(value, new JsonSerializerOptions
			{
				DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
				WriteIndented = false
			});

			using var doc = JsonDocument.Parse(json);
			return doc.RootElement.Clone();
		}
        else
        {
            var dictionary = new Dictionary<string, object?>();
            var type = value.GetType();

            foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (!prop.CanRead) continue;
                if (prop.GetIndexParameters().Length > 0) continue;

                var propType = prop.PropertyType;

                if (IsScalarType(propType))
                {
                    try
                    {
                        var propValue = prop.GetValue(value);
                        dictionary[prop.Name] = propValue;
                    }
                    catch
                    {
                        // Skip properties that throw on access
                    }
                }
            }

            var json = JsonSerializer.Serialize(dictionary, DefaultOptions);
            using var doc = JsonDocument.Parse(json);
            return doc.RootElement.Clone();
        }
    }

    #endregion

    #region Deep Serialization (New)

    /// <summary>
    /// Converts an object to a deep JsonElement with type name for audit logging.
    /// Includes nested objects and collections up to the specified depth.
    /// </summary>
    /// <param name="value">The object to serialize</param>
    /// <param name="maxDepth">Maximum depth for nested objects (default: 5)</param>
    public static AuditData ToDeepAuditData<T>(this T value, int maxDepth = DefaultMaxDepth)
    {
        var typeName = typeof(T).Name;

        if (typeName.StartsWith("<>"))
        {
            typeName = "AuditData";
        }

        var jsonElement = value.ToDeepJsonElement(maxDepth);
        return new AuditData(jsonElement, typeName);
    }

    /// <summary>
    /// Converts an object to a deep JsonElement with a custom type name for audit logging.
    /// Includes nested objects and collections up to the specified depth.
    /// </summary>
    /// <param name="value">The object to serialize</param>
    /// <param name="typeName">Custom type name for the audit data</param>
    /// <param name="maxDepth">Maximum depth for nested objects (default: 5)</param>
    public static AuditData ToDeepAuditData<T>(this T value, string typeName, int maxDepth = DefaultMaxDepth)
    {
        var jsonElement = value.ToDeepJsonElement(maxDepth);
        return new AuditData(jsonElement, typeName);
    }

    /// <summary>
    /// Converts an object to a JsonElement including nested objects and collections.
    /// </summary>
    /// <param name="value">The object to serialize</param>
    /// <param name="maxDepth">Maximum depth for nested objects (default: 5)</param>
    public static JsonElement ToDeepJsonElement<T>(this T value, int maxDepth = DefaultMaxDepth)
    {
        if (value == null)
        {
            return JsonDocument.Parse("null").RootElement.Clone();
        }

        var visited = new HashSet<object>(ReferenceEqualityComparer.Instance);
        var result = SerializeDeep(value, 0, maxDepth, visited);

        var json = JsonSerializer.Serialize(result, DefaultOptions);
        using var doc = JsonDocument.Parse(json);
        return doc.RootElement.Clone();
    }

    private static object? SerializeDeep(object? value, int currentDepth, int maxDepth, HashSet<object> visited)
    {
        if (value == null)
        {
            return null;
        }

        var type = value.GetType();

        // Handle scalar types directly
        if (IsScalarType(type))
        {
            return value;
        }

        // Check depth limit
        if (currentDepth >= maxDepth)
        {
            // Return a placeholder or type name for objects that exceed depth
            if (IsCollectionType(type))
            {
                return $"[Collection truncated at depth {maxDepth}]";
            }
            return $"[{type.Name} truncated at depth {maxDepth}]";
        }

        // Handle circular references
        if (!type.IsValueType && visited.Contains(value))
        {
            return $"[Circular reference to {type.Name}]";
        }

        // Track reference types to detect cycles
        if (!type.IsValueType)
        {
            visited.Add(value);
        }

        try
        {
            // Handle strings (already handled by IsScalarType, but being explicit)
            if (value is string stringValue)
            {
                return stringValue;
            }

            // Handle dictionaries
            if (value is IDictionary dictionary)
            {
                return SerializeDictionary(dictionary, currentDepth, maxDepth, visited);
            }

            // Handle collections (arrays, lists, etc.)
            if (IsCollectionType(type))
            {
                return SerializeCollection((IEnumerable)value, currentDepth, maxDepth, visited);
            }

            // Handle complex objects
            return SerializeObject(value, currentDepth, maxDepth, visited);
        }
        finally
        {
            // Remove from visited set when done (allows same object in different branches)
            if (!type.IsValueType)
            {
                visited.Remove(value);
            }
        }
    }

    private static Dictionary<string, object?> SerializeObject(object value, int currentDepth, int maxDepth, HashSet<object> visited)
    {
        var dictionary = new Dictionary<string, object?>();
        var type = value.GetType();

        foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (!prop.CanRead) continue;
            if (prop.GetIndexParameters().Length > 0) continue;

            // Skip properties with JsonIgnore attribute
            if (prop.GetCustomAttribute<JsonIgnoreAttribute>() != null) continue;

            try
            {
                var propValue = prop.GetValue(value);
                dictionary[prop.Name] = SerializeDeep(propValue, currentDepth + 1, maxDepth, visited);
            }
            catch
            {
                // Skip properties that throw on access
            }
        }

        return dictionary;
    }

    private static List<object?> SerializeCollection(IEnumerable collection, int currentDepth, int maxDepth, HashSet<object> visited)
    {
        var list = new List<object?>();

        foreach (var item in collection)
        {
            list.Add(SerializeDeep(item, currentDepth + 1, maxDepth, visited));
        }

        return list;
    }

    private static Dictionary<string, object?> SerializeDictionary(IDictionary dictionary, int currentDepth, int maxDepth, HashSet<object> visited)
    {
        var result = new Dictionary<string, object?>();

        foreach (DictionaryEntry entry in dictionary)
        {
            var key = entry.Key?.ToString() ?? "null";
            result[key] = SerializeDeep(entry.Value, currentDepth + 1, maxDepth, visited);
        }

        return result;
    }

    #endregion

    #region Export Audit Data

    /// <summary>
    /// Converts a byte array to a JsonElement containing the Base64-encoded string.
    /// </summary>
    public static JsonElement BytesToJsonElement(byte[] bytes)
    {
        if (bytes == null || bytes.Length == 0)
        {
            return JsonDocument.Parse("null").RootElement.Clone();
        }

        // System.Text.Json automatically Base64-encodes byte arrays
        var json = JsonSerializer.Serialize(bytes);
        using var doc = JsonDocument.Parse(json);
        return doc.RootElement.Clone();
    }

    /// <summary>
    /// Creates audit data containing export metadata, optional filters, and optional raw file content.
    /// </summary>
    public static AuditData ToExportAuditData(
        string fileName,
        byte[] fileContent,
        int recordCount,
        object? filters = null,
        bool includeFileContent = false)
    {
        var metadata = new Dictionary<string, object?>
        {
            ["FileName"] = fileName,
            ["FileSizeBytes"] = fileContent.Length,
            ["RecordCount"] = recordCount,
            ["ExportedAt"] = DateTime.UtcNow
        };

        // Include filters if provided
        if (filters != null)
        {
            metadata["Filters"] = filters;
        }

        // Include file content if requested
        if (includeFileContent)
        {
            metadata["FileContentBase64"] = Convert.ToBase64String(fileContent);
        }

        var json = JsonSerializer.Serialize(metadata, new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            WriteIndented = false
        });

        using var doc = JsonDocument.Parse(json);
        var element = doc.RootElement.Clone();

        var typeName = includeFileContent ? "CsvExportWithContent" : "CsvExportMetadata";
        return new AuditData(element, typeName);
    }

    #endregion

    #region Helper Methods

    private static bool IsScalarType(Type type)
    {
        var underlyingType = Nullable.GetUnderlyingType(type);
        if (underlyingType != null)
        {
            type = underlyingType;
        }

        return type.IsPrimitive
            || type.IsEnum
            || type == typeof(string)
            || type == typeof(decimal)
            || type == typeof(DateTime)
            || type == typeof(DateTimeOffset)
            || type == typeof(DateOnly)
            || type == typeof(TimeOnly)
            || type == typeof(TimeSpan)
            || type == typeof(Guid);
    }

    private static bool IsCollectionType(Type type)
    {
        if (type == typeof(string))
        {
            return false; // string implements IEnumerable but we treat it as scalar
        }

        return typeof(IEnumerable).IsAssignableFrom(type);
    }

    #endregion
}