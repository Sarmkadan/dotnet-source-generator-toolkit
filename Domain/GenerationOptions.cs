#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using Microsoft.CodeAnalysis.Diagnostics;

namespace DotNetSourceGeneratorToolkit.Domain;

/// <summary>
/// Controls nullable context for generated code files.
/// </summary>
public enum NullableContext
{
    /// <summary>
    /// No nullable context directive is added to generated files.
    /// </summary>
    None,

    /// <summary>
    /// Adds #nullable enable directive to generated files.
    /// </summary>
    Enable,

    /// <summary>
    /// Adds #nullable disable directive to generated files.
    /// </summary>
    Disable
}

/// <summary>
/// Controls namespace declaration style for generated code files.
/// </summary>
public enum NamespaceStyle
{
    /// <summary>
    /// Traditional namespace declaration with braces: namespace MyNamespace { ... }
    /// </summary>
    Braced,

    /// <summary>
    /// File-scoped namespace declaration: namespace MyNamespace;
    /// </summary>
    FileScoped
}

/// <summary>
/// Controls whether the [GeneratedCode] attribute is emitted for generated files.
/// </summary>
public enum GeneratedCodeAttributeMode
{
    /// <summary>
    /// Never emit the [GeneratedCode] attribute.
    /// </summary>
    Never,

    /// <summary>
    /// Always emit the [GeneratedCode] attribute.
    /// </summary>
    Always,

    /// <summary>
    /// Emit the [GeneratedCode] attribute only when the toolkit is generating the code.
    /// </summary>
    Conditional
}

/// <summary>
/// Configuration options for code generation that control cross-cutting emission policies.
/// These options can be configured via MSBuild properties or AnalyzerConfigOptionsProvider.
/// </summary>
/// <param name="NullableContext">Controls nullable context directives in generated files.</param>
/// <param name="NamespaceStyle">Controls namespace declaration style (braced vs file-scoped).</param>
/// <param name="EmitGeneratedCodeAttribute">Controls whether [GeneratedCode] attribute is emitted.</param>
/// <param name="HeaderComment">Optional custom header comment for generated files.</param>
/// <param name="LangVersion">Target C# language version for generated code.</param>
/// <param name="IndentSize">Indentation size in spaces.</param>
public sealed record GenerationOptions
{
    /// <summary>
    /// Controls nullable context directives in generated files.
    /// </summary>
    public NullableContext NullableContext { get; init; } = NullableContext.Enable;

    /// <summary>
    /// Controls namespace declaration style (braced vs file-scoped).
    /// </summary>
    public NamespaceStyle NamespaceStyle { get; init; } = NamespaceStyle.Braced;

    /// <summary>
    /// Controls whether [GeneratedCode] attribute is emitted.
    /// </summary>
    public GeneratedCodeAttributeMode EmitGeneratedCodeAttribute { get; init; } = GeneratedCodeAttributeMode.Always;

    /// <summary>
    /// Optional custom header comment for generated files.
    /// </summary>
    public string? HeaderComment { get; init; }

    /// <summary>
    /// Target C# language version for generated code.
    /// </summary>
    public string? LangVersion { get; init; }

    /// <summary>
    /// Indentation size in spaces.
    /// </summary>
    public int IndentSize { get; init; } = 4;

    /// <summary>
    /// Default generation options with all features enabled.
    /// </summary>
    public static readonly GenerationOptions Default = new()
    {
        NullableContext = NullableContext.Enable,
        NamespaceStyle = NamespaceStyle.Braced,
        EmitGeneratedCodeAttribute = GeneratedCodeAttributeMode.Always,
        IndentSize = 4
    };

    /// <summary>
    /// Minimal generation options with no nullable context and no headers.
    /// </summary>
    public static readonly GenerationOptions Minimal = new()
    {
        NullableContext = NullableContext.None,
        HeaderComment = null
    };

    /// <summary>
    /// File-scoped namespace generation options.
    /// </summary>
    public static readonly GenerationOptions FileScoped = new()
    {
        NamespaceStyle = NamespaceStyle.FileScoped
    };

    /// <summary>
    /// Validates the generation options.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when indent size is less than 1.</exception>
    public void Validate()
    {
        if (IndentSize < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(IndentSize), "Indent size must be at least 1");
        }
    }

    /// <summary>
    /// Gets the effective nullable directive for the given options.
    /// </summary>
    /// <returns>Nullable directive string or empty if None.</returns>
    public string GetNullableDirective()
    {
        return NullableContext switch
        {
            NullableContext.Enable => "#nullable enable",
            NullableContext.Disable => "#nullable disable",
            _ => string.Empty
        };
    }

    /// <summary>
    /// Gets the effective namespace declaration style.
    /// </summary>
    /// <returns>Namespace declaration string.</returns>
    public string GetNamespaceDeclaration(string namespaceName)
    {
        return NamespaceStyle switch
        {
            NamespaceStyle.FileScoped => $"namespace {namespaceName};\n",
            _ => $"namespace {namespaceName}\n{{\n"
        };
    }

    /// <summary>
    /// Gets the effective namespace closing brace.
    /// </summary>
    /// <returns>Closing brace string or empty if file-scoped.</returns>
    public string GetNamespaceClosingBrace()
    {
        return NamespaceStyle == NamespaceStyle.FileScoped ? string.Empty : "}";
    }

    /// <summary>
    /// Gets the effective [GeneratedCode] attribute declaration.
    /// </summary>
    /// <param name="generatorName">Name of the generator producing the code.</param>
    /// <param name="generatorVersion">Version of the generator.</param>
    /// <returns>[GeneratedCode] attribute string or empty if Never.</returns>
    public string GetGeneratedCodeAttribute(string generatorName, string generatorVersion)
    {
        return EmitGeneratedCodeAttribute switch
        {
            GeneratedCodeAttributeMode.Always =>
                $"[global::System.CodeDom.Compiler.GeneratedCode(\"{generatorName}\", \"{generatorVersion}\")]\n",
            GeneratedCodeAttributeMode.Conditional =>
                $"#if NET6_0_OR_GREATER\n[global::System.CodeDom.Compiler.GeneratedCode(\"{generatorName}\", \"{generatorVersion}\")]\n#endif\n",
            _ => string.Empty
        };
    }

    /// <summary>
    /// Gets the effective header comment for generated files.
    /// </summary>
    /// <param name="author">Author information.</param>
    /// <param name="description">Optional file description.</param>
    /// <returns>Header comment string or empty if no header is configured.</returns>
    public string GetHeaderComment(string author = "Vladyslav Zaiets | https://sarmkadan.com", string description = null)
    {
        if (string.IsNullOrWhiteSpace(HeaderComment))
        {
            return string.Empty;
        }

        var lines = new System.Collections.Generic.List<string>
        {
            "// =============================================================================",
            "// " + HeaderComment,
            "// ============================================================================="
        };

        if (!string.IsNullOrWhiteSpace(description))
        {
            lines.Add("//");
            lines.Add("// " + description);
        }

        return string.Join(Environment.NewLine, lines) + Environment.NewLine + Environment.NewLine;
    }

    /// <summary>
    /// Gets the effective language version directive.
    /// </summary>
    /// <returns>Language version directive or empty if not specified.</returns>
    public string GetLangVersionDirective()
    {
        return string.IsNullOrWhiteSpace(LangVersion) ? string.Empty : $"#pragma warning disable CS8901 // Target runtime mono won't support this syntax\n#langversion {LangVersion}\n";
    }
}