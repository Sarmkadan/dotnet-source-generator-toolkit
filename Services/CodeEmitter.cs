#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Collections.Generic;
using System.Linq;

namespace DotNetSourceGeneratorToolkit.Services;

/// <summary>
/// Helper class for generating C# code with consistent formatting and structure.
/// Provides reusable methods for creating common code patterns like file headers,
/// namespace wrappers, and usings declarations.
/// </summary>
internal static class CodeEmitter
{
    /// <summary>
    /// Generates the standard file header with author information and copyright.
    /// </summary>
    /// <param name="author">The author name and website.</param>
    /// <param name="description">Optional description of the generated file.</param>
    /// <returns>Formatted file header as a string.</returns>
    public static string GenerateFileHeader(string author = "Vladyslav Zaiets | https://sarmkadan.com", string description = null)
    {
        var lines = new List<string>
        {
            "// =============================================================================",
            "// Author: " + author,
            "// CTO & Software Architect",
            "// ============================================================================="
        };

        if (!string.IsNullOrWhiteSpace(description))
        {
            lines.Add("//");
            lines.Add("// " + description);
        }

        return string.Join(Environment.NewLine, lines);
    }

    /// <summary>
    /// Generates the standard usings block for generator service files.
    /// </summary>
    /// <param name="additionalUsings">Additional using directives to include.</param>
    /// <returns>Formatted usings block as a string.</returns>
    public static string GenerateUsings(params string[] additionalUsings)
    {
        var usings = new List<string>
        {
            "using System;",
            "using System.Collections.Generic;",
            "using System.Linq;",
            "using System.Threading.Tasks;"
        };

        if (additionalUsings != null && additionalUsings.Length > 0)
        {
            usings.AddRange(additionalUsings);
        }

        return string.Join(Environment.NewLine, usings.Select(u => u + ";"));
    }

    /// <summary>
    /// Wraps generated code in a namespace declaration.
    /// </summary>
    /// <param name="namespace">The target namespace.</param>
    /// <param name="code">The code to wrap.</param>
    /// <param name="indentLevel">Optional indentation level (default: 0).</param>
    /// <returns>Code wrapped in namespace declaration.</returns>
    public static string WrapInNamespace(string ns, string code, int indentLevel = 0)
    {
        var indent = GetIndent(indentLevel);
        var closeIndent = GetIndent(indentLevel);

        return $@"{indent}namespace {ns}
{indent}{{
{code}
{closeIndent}}}";
    }

    /// <summary>
    /// Generates a class declaration with optional modifiers, base classes, and documentation.
    /// </summary>
    /// <param name="className">Name of the class.</param>
    /// <param name="modifiers">Access modifiers (e.g., "public sealed").</param>
    /// <param name="baseTypes">Optional base types/interfaces.</param>
    /// <param name="documentation">Optional XML documentation.</param>
    /// <param name="body">Class body content.</param>
    /// <param name="indentLevel">Indentation level.</param>
    /// <returns>Formatted class declaration.</returns>
    public static string GenerateClassDeclaration(
        string className,
        string modifiers = "public sealed",
        string[] baseTypes = null,
        string documentation = null,
        string body = null,
        int indentLevel = 0)
    {
        var indent = GetIndent(indentLevel);
        var lines = new List<string>();

        if (!string.IsNullOrWhiteSpace(documentation))
        {
            lines.Add(indent + "/// <summary>");
            lines.Add(indent + "/// " + documentation);
            lines.Add(indent + "/// </summary>");
        }

        var declaration = $"{indent}{modifiers} class {className}";

        if (baseTypes != null && baseTypes.Length > 0)
        {
            declaration += " : " + string.Join(", ", baseTypes);
        }

        lines.Add(indent + declaration);
        lines.Add(indent + "{");

        if (!string.IsNullOrWhiteSpace(body))
        {
            lines.Add(body);
        }

        lines.Add(indent + "}");

        return string.Join(Environment.NewLine, lines);
    }

    /// <summary>
    /// Generates a method declaration with optional modifiers, parameters, and body.
    /// </summary>
    /// <param name="methodName">Method name.</param>
    /// <param name="returnType">Return type.</param>
    /// <param name="parameters">Method parameters.</param>
    /// <param name="modifiers">Access modifiers.</param>
    /// <param name="documentation">XML documentation.</param>
    /// <param name="body">Method body.</param>
    /// <param name="indentLevel">Indentation level.</param>
    /// <returns>Formatted method declaration.</returns>
    public static string GenerateMethodDeclaration(
        string methodName,
        string returnType,
        string parameters = null,
        string modifiers = "public async",
        string documentation = null,
        string body = null,
        int indentLevel = 1)
    {
        var indent = GetIndent(indentLevel);
        var lines = new List<string>();

        if (!string.IsNullOrWhiteSpace(documentation))
        {
            lines.Add(indent + "/// <summary>");
            lines.Add(indent + "/// " + documentation);
            lines.Add(indent + "/// </summary>");
        }

        var declaration = $"{indent}{modifiers} {returnType} {methodName}({parameters})";
        lines.Add(indent + declaration);
        lines.Add(indent + "{");

        if (!string.IsNullOrWhiteSpace(body))
        {
            lines.Add(body);
        }

        lines.Add(indent + "}");

        return string.Join(Environment.NewLine, lines);
    }

    /// <summary>
    /// Gets indentation string for the specified level.
    /// </summary>
    /// <param name="level">Indentation level (0 = no indent, 1 = 4 spaces, etc.).</param>
    /// <returns>Indentation string.</returns>
    public static string GetIndent(int level)
    {
        return new string(' ', level * 4);
    }

    /// <summary>
    /// Creates a property declaration.
    /// </summary>
    /// <param name="propertyType">Property type.</param>
    /// <param name="propertyName">Property name.</param>
    /// <param name="accessors">Property accessors (e.g., "get; set;").</param>
    /// <param name="modifiers">Access modifiers.</param>
    /// <param name="documentation">XML documentation.</param>
    /// <param name="indentLevel">Indentation level.</param>
    /// <returns>Formatted property declaration.</returns>
    public static string GeneratePropertyDeclaration(
        string propertyType,
        string propertyName,
        string accessors = "get; set;",
        string modifiers = "",
        string documentation = null,
        int indentLevel = 1)
    {
        var indent = GetIndent(indentLevel);
        var lines = new List<string>();

        if (!string.IsNullOrWhiteSpace(documentation))
        {
            lines.Add(indent + "/// <summary>");
            lines.Add(indent + "/// " + documentation);
            lines.Add(indent + "/// </summary>");
        }

        var declaration = $"{indent}{modifiers} {propertyType} {propertyName} {{ {accessors} }}";
        lines.Add(declaration);

        return string.Join(Environment.NewLine, lines);
    }

    /// <summary>
    /// Creates a field declaration.
    /// </summary>
    /// <param name="fieldType">Field type.</param>
    /// <param name="fieldName">Field name.</param>
    /// <param name="initializer">Optional initializer.</param>
    /// <param name="modifiers">Access modifiers.</param>
    /// <param name="indentLevel">Indentation level.</param>
    /// <returns>Formatted field declaration.</returns>
    public static string GenerateFieldDeclaration(
        string fieldType,
        string fieldName,
        string initializer = null,
        string modifiers = "private readonly",
        int indentLevel = 1)
    {
        var indent = GetIndent(indentLevel);
        var declaration = $"{indent}{modifiers} {fieldType} {fieldName}";

        if (!string.IsNullOrWhiteSpace(initializer))
        {
            declaration += " = " + initializer + ";";
        }
        else
        {
            declaration += ";";
        }

        return declaration;
    }

    /// <summary>
    /// Creates a variable declaration.
    /// </summary>
    /// <param name="variableType">Variable type.</param>
    /// <param name="variableName">Variable name.</param>
    /// <param name="initializer">Initializer expression.</param>
    /// <param name="indentLevel">Indentation level.</param>
    /// <returns>Formatted variable declaration.</returns>
    public static string GenerateVariableDeclaration(
        string variableType,
        string variableName,
        string initializer,
        int indentLevel = 1)
    {
        var indent = GetIndent(indentLevel);
        return $"{indent}{variableType} {variableName} = {initializer};";
    }

    /// <summary>
    /// Creates a comment block.
    /// </summary>
    /// <param name="commentText">Comment text.</param>
    /// <param name="indentLevel">Indentation level.</param>
    /// <returns>Formatted comment block.</returns>
    public static string GenerateComment(string commentText, int indentLevel = 0)
    {
        var indent = GetIndent(indentLevel);
        return indent + "// " + commentText;
    }

    /// <summary>
    /// Creates a multi-line string literal with proper indentation.
    /// </summary>
    /// <param name="content">Content lines.</param>
    /// <param name="indentLevel">Indentation level.</param>
    /// <returns>Formatted multi-line string.</returns>
    public static string GenerateMultilineString(IEnumerable<string> content, int indentLevel = 0)
    {
        var indent = GetIndent(indentLevel);
        return string.Join(Environment.NewLine, content.Select(line => indent + line));
    }
}