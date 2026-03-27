// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNetSourceGeneratorToolkit.Domain;
using Microsoft.Extensions.Logging;

namespace DotNetSourceGeneratorToolkit.Services;

/// <summary>
/// Generates serializer classes for converting entities to/from various formats
/// including JSON, XML, and binary representations.
/// </summary>
public class SerializerGeneratorService : ISerializerGeneratorService
{
    private readonly ILogger<SerializerGeneratorService> _logger;

    public SerializerGeneratorService(ILogger<SerializerGeneratorService> logger)
    {
        _logger = logger;
    }

    public async Task<IEnumerable<GenerationResult>> GenerateAllSerializersAsync(List<Entity> entities)
    {
        if (entities == null || entities.Count == 0)
            throw new ArgumentException("Entities collection cannot be null or empty");

        _logger.LogInformation("Generating serializers for {Count} entities", entities.Count);

        var results = new List<GenerationResult>();

        foreach (var entity in entities)
        {
            // Generate JSON serializer
            var jsonResult = await GenerateSerializerAsync(entity, SerializerFormat.Json);
            results.Add(jsonResult);

            // Generate XML serializer
            var xmlResult = await GenerateSerializerAsync(entity, SerializerFormat.Xml);
            results.Add(xmlResult);
        }

        var successCount = results.Count(r => r.Status == GenerationStatus.Completed);
        _logger.LogInformation("Generated {Success}/{Total} serializers", successCount, results.Count);

        return results;
    }

    public async Task<GenerationResult> GenerateSerializerAsync(Entity entity, SerializerFormat format)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        var formatName = format.ToString();
        _logger.LogInformation("Generating {Format} serializer for entity: {EntityName}", formatName, entity.Name);

        var result = new GenerationResult
        {
            EntityName = entity.Name,
            GeneratorType = GeneratorType.Serializer,
            Status = GenerationStatus.InProgress,
            Metadata = new Dictionary<string, string> { { "Format", formatName } },
        };

        try
        {
            var code = format switch
            {
                SerializerFormat.Json => GenerateJsonSerializerCode(entity),
                SerializerFormat.Xml => GenerateXmlSerializerCode(entity),
                SerializerFormat.Binary => GenerateBinarySerializerCode(entity),
                _ => throw new NotSupportedException($"Serializer format not supported: {format}"),
            };

            result.GeneratedCode = code;
            result.OutputFilePath = Path.Combine("Serializers", $"{entity.Name}{formatName}Serializer.cs");
            result.MarkAsCompleted(GenerationStatus.Completed, 200);

            _logger.LogInformation("Serializer generated: {EntityName} ({Format})", entity.Name, formatName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Serializer generation failed for: {EntityName}", entity.Name);
            result.AddError(ex.Message);
            result.MarkAsCompleted(GenerationStatus.Failed, 0);
        }

        return await Task.FromResult(result);
    }

    private string GenerateJsonSerializerCode(Entity entity)
    {
        var properties = entity.Properties;
        var jsonProperties = string.Join(",\\n", properties.Select(p =>
            $"                \\\"{p.Name.ToCamelCase()}\\\": entity.{p.Name}"));

        return $@"// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text.Json;
using System.Text.Json.Serialization;

namespace {entity.Namespace}.Serializers
{{
    /// <summary>
    /// JSON serializer for {entity.Name} entity.
    /// Provides methods for converting entities to/from JSON format.
    /// </summary>
    public class {entity.Name}JsonSerializer
    {{
        private static readonly JsonSerializerOptions Options = new()
        {{
            PropertyNameCaseInsensitive = true,
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        }};

        /// <summary>Serializes an entity to JSON string.</summary>
        public static string Serialize({entity.Name} entity)
        {{
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            return JsonSerializer.Serialize(entity, Options);
        }}

        /// <summary>Deserializes JSON string to entity.</summary>
        public static {entity.Name} Deserialize(string json)
        {{
            if (string.IsNullOrWhiteSpace(json))
                throw new ArgumentNullException(nameof(json));

            var entity = JsonSerializer.Deserialize<{entity.Name}>(json, Options);
            return entity ?? throw new InvalidOperationException(""Failed to deserialize JSON"");
        }}

        /// <summary>Serializes entity to JsonElement.</summary>
        public static JsonElement ToJsonElement({entity.Name} entity)
        {{
            var json = Serialize(entity);
            return JsonDocument.Parse(json).RootElement;
        }}
    }}
}}";
    }

    private string GenerateXmlSerializerCode(Entity entity)
    {
        var properties = entity.Properties;
        var xmlElements = string.Join(Environment.NewLine,
            properties.Select(p => $"            var {p.Name.ToLower()}Element = element.CreateElement(\"{p.Name}\");"));

        return $@"// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Xml;
using System.Xml.Linq;

namespace {entity.Namespace}.Serializers
{{
    /// <summary>
    /// XML serializer for {entity.Name} entity.
    /// Provides methods for converting entities to/from XML format.
    /// </summary>
    public class {entity.Name}XmlSerializer
    {{
        private const string RootElementName = ""{entity.Name}"";

        /// <summary>Serializes an entity to XML string.</summary>
        public static string Serialize({entity.Name} entity)
        {{
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            var doc = new XDocument(
                new XElement(RootElementName,
{string.Join(Environment.NewLine, properties.Select(p => $"                    new XElement(\"{p.Name}\", entity.{p.Name})"))}
                )
            );

            return doc.ToString();
        }}

        /// <summary>Deserializes XML string to entity.</summary>
        public static {entity.Name} Deserialize(string xml)
        {{
            if (string.IsNullOrWhiteSpace(xml))
                throw new ArgumentNullException(nameof(xml));

            var doc = XDocument.Parse(xml);
            var root = doc.Root;

            if (root?.Name != RootElementName)
                throw new InvalidOperationException($""Root element must be {{RootElementName}}"");

            var entity = new {entity.Name}();

{string.Join(Environment.NewLine, properties.Select(p => $"            entity.{p.Name} = root.Element(\"{p.Name}\")?.Value;"))}

            return entity;
        }}
    }}
}}";
    }

    private string GenerateBinarySerializerCode(Entity entity)
    {
        return $@"// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace {entity.Namespace}.Serializers
{{
    /// <summary>
    /// Binary serializer for {entity.Name} entity.
    /// Provides methods for converting entities to/from binary format.
    /// </summary>
    public class {entity.Name}BinarySerializer
    {{
        /// <summary>Serializes an entity to binary byte array.</summary>
        public static byte[] Serialize({entity.Name} entity)
        {{
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            using (var ms = new MemoryStream())
            {{
                var formatter = new BinaryFormatter();
                formatter.Serialize(ms, entity);
                return ms.ToArray();
            }}
        }}

        /// <summary>Deserializes binary byte array to entity.</summary>
        public static {entity.Name} Deserialize(byte[] data)
        {{
            if (data == null || data.Length == 0)
                throw new ArgumentNullException(nameof(data));

            using (var ms = new MemoryStream(data))
            {{
                var formatter = new BinaryFormatter();
                return (BinaryFormatter)formatter.Deserialize(ms);
            }}
        }}

        /// <summary>Gets the size in bytes of serialized entity.</summary>
        public static int GetSerializedSize({entity.Name} entity)
        {{
            var data = Serialize(entity);
            return data.Length;
        }}
    }}
}}";
    }
}

/// <summary>
/// Helper extension methods for string manipulation.
/// </summary>
public static class StringExtensions
{
    public static string ToCamelCase(this string str)
    {
        if (string.IsNullOrEmpty(str) || char.IsLower(str[0]))
            return str;

        return char.ToLower(str[0]) + str[1..];
    }
}
