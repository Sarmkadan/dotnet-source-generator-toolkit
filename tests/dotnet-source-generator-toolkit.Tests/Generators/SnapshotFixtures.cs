#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNetSourceGeneratorToolkit.Domain;

namespace DotNetSourceGeneratorToolkit.Tests.Generators;

/// <summary>
/// Shared, deterministic entity fixtures used by the generator-service snapshot tests.
/// The shapes are kept fixed so generated output is stable across runs.
/// </summary>
internal static class SnapshotFixtures
{
    /// <summary>A small, stable Product entity with an int primary key and two properties.</summary>
    public static Entity Product()
    {
        var entity = new Entity
        {
            Name = "Product",
            Namespace = "Shop.Domain",
            TableName = "products",
        };
        entity.AddProperty(new EntityProperty
        {
            Name = "Id",
            Type = "int",
            IsPrimaryKey = true,
            IsRequired = true,
        });
        entity.AddProperty(new EntityProperty
        {
            Name = "Name",
            Type = "string",
            IsRequired = true,
            MaxLength = 100,
        });
        return entity;
    }
}
