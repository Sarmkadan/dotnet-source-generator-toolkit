#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNetSourceGeneratorToolkit.Domain;

namespace DotNetSourceGeneratorToolkit.Benchmarks;

/// <summary>
/// Sample entities for benchmarking different generation scenarios
/// </summary>
public static class BenchmarkEntities
{
    /// <summary>
    /// Simple entity with minimal properties - baseline for comparison
    /// </summary>
    [Repository]
    [Mapper]
    [Validator]
    public sealed class SimpleEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Entity with multiple properties - tests property parsing performance
    /// </summary>
    [Repository]
    [Mapper]
    [Validator]
    public sealed class ComplexEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public bool IsActive { get; set; } = true;
        public string Category { get; set; } = string.Empty;
        public double Rating { get; set; }
        public int Views { get; set; }
    }

    /// <summary>
    /// Entity with nested generic types - tests complex type parsing
    /// </summary>
    [Repository]
    [Mapper]
    [Validator]
    public sealed class GenericEntity
    {
        public int Id { get; set; }
        public List<string> Tags { get; set; } = [];
        public Dictionary<string, object> Metadata { get; set; } = [];
        public string[] Aliases { get; set; } = [];
        public HashSet<int> RelatedIds { get; set; } = [];
    }

    /// <summary>
    /// Entity with many properties - tests scalability
    /// </summary>
    [Repository]
    [Mapper]
    [Validator]
    public sealed class LargeEntity
    {
        public int Id { get; set; }
        public string Property1 { get; set; } = string.Empty;
        public string Property2 { get; set; } = string.Empty;
        public string Property3 { get; set; } = string.Empty;
        public string Property4 { get; set; } = string.Empty;
        public string Property5 { get; set; } = string.Empty;
        public string Property6 { get; set; } = string.Empty;
        public string Property7 { get; set; } = string.Empty;
        public string Property8 { get; set; } = string.Empty;
        public string Property9 { get; set; } = string.Empty;
        public string Property10 { get; set; } = string.Empty;
        public int IntProperty1 { get; set; }
        public int IntProperty2 { get; set; }
        public int IntProperty3 { get; set; }
        public decimal DecimalProperty1 { get; set; }
        public decimal DecimalProperty2 { get; set; }
        public bool BoolProperty1 { get; set; }
        public bool BoolProperty2 { get; set; }
        public DateTime DateTimeProperty1 { get; set; }
        public DateTime DateTimeProperty2 { get; set; }
    }

    /// <summary>
    /// Entity with serializer attribute - tests serialization generation
    /// </summary>
    [Serializer(Formats = new[] { "Json", "Xml", "Csv" })]
    public sealed class SerializableEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Value { get; set; }
        public DateTime Timestamp { get; set; }
    }

    /// <summary>
    /// Entity with all attributes - comprehensive test
    /// </summary>
    [Repository]
    [Mapper]
    [Validator]
    [Serializer(Formats = new[] { "Json" })]
    public sealed class FullFeaturedEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public bool IsActive { get; set; } = true;
    }
}