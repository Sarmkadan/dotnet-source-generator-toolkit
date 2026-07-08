#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

// Marker attributes mirroring the toolkit's attribute-driven generation model.
// The toolkit's analyzers match on attribute name (see GenerationConstants and
// SourceGeneratorService), so target projects declare their own local copies
// of these markers. These are used to annotate the benchmark entities below.

namespace DotNetSourceGeneratorToolkit.Benchmarks;

/// <summary>
/// Marks a class as a repository generation target for the source generator toolkit.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class RepositoryAttribute : Attribute
{
}

/// <summary>
/// Marks a class as a mapper generation target for the source generator toolkit.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class MapperAttribute : Attribute
{
}

/// <summary>
/// Marks a class as a validator generation target for the source generator toolkit.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class ValidatorAttribute : Attribute
{
}

/// <summary>
/// Marks a class as a serializer generation target for the source generator toolkit,
/// specifying which output formats to generate.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class SerializerAttribute : Attribute
{
    public string[] Formats { get; set; } = Array.Empty<string>();
}
