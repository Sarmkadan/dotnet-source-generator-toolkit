#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using DotNetSourceGeneratorToolkit.Domain;

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
    /// Generates the standard file header with author information and copyright using GenerationOptions.
    /// </summary>
    /// <param name="options">Generation options controlling header format.</param>
    /// <param name="author">The author name and website.</param>
    /// <param name="description">Optional description of the generated file.</param>
    /// <returns>Formatted file header as a string.</returns>
    public static string GenerateFileHeader(GenerationOptions options, string author = "Vladyslav Zaiets | https://sarmkadan.com", string description = null)
    {
        ArgumentNullException.ThrowIfNull(options);

        return options.GetHeaderComment(author, description);
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

        return indent + "namespace " + ns + Environment.NewLine +
               indent + "{" + Environment.NewLine +
               code + Environment.NewLine +
               closeIndent + "}";
    }

    /// <summary>
    /// Wraps generated code in a namespace declaration using GenerationOptions.
    /// </summary>
    /// <param name="options">Generation options controlling namespace style.</param>
    /// <param name="namespace">The target namespace.</param>
    /// <param name="code">The code to wrap.</param>
    /// <param name="indentLevel">Optional indentation level (default: 0).</param>
    /// <returns>Code wrapped in namespace declaration.</returns>
    public static string WrapInNamespace(GenerationOptions options, string ns, string code, int indentLevel = 0)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentException.ThrowIfNullOrEmpty(ns);

        var indent = GetIndent(indentLevel);
        var namespaceDecl = options.GetNamespaceDeclaration(ns);
        var closingBrace = options.GetNamespaceClosingBrace();

        if (string.IsNullOrEmpty(closingBrace))
        {
            return namespaceDecl + code;
        }

        return namespaceDecl + indent + "{" + Environment.NewLine +
               code + Environment.NewLine +
               indent + closingBrace;
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

        var escapedClassName = EscapeIdentifier(className);
        var declaration = indent + modifiers + " class " + escapedClassName;

        if (baseTypes != null && baseTypes.Length > 0)
        {
            var formattedBaseTypes = baseTypes.Select(FormatTypeName).ToArray();
            declaration += " : " + string.Join(", ", formattedBaseTypes);
        }

        lines.Add(declaration);
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

        var escapedMethodName = EscapeIdentifier(methodName);
        var formattedReturnType = FormatTypeName(returnType);
        var declaration = indent + modifiers + " " + formattedReturnType + " " + escapedMethodName + "(" + parameters + ")";
        lines.Add(declaration);
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

        var escapedPropertyName = EscapeIdentifier(propertyName);
        var formattedPropertyType = FormatTypeName(propertyType);
        var declaration = indent + modifiers + " " + formattedPropertyType + " " + escapedPropertyName + " { " + accessors + " }";
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
        var escapedFieldName = EscapeIdentifier(fieldName);
        var formattedFieldType = FormatTypeName(fieldType);
        var declaration = indent + modifiers + " " + formattedFieldType + " " + escapedFieldName;

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
        var escapedVariableName = EscapeIdentifier(variableName);
        var formattedVariableType = FormatTypeName(variableType);
        return indent + formattedVariableType + " " + escapedVariableName + " = " + initializer + ";";
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

    /// <summary>
    /// Escapes a C# identifier if it's a keyword or contains invalid characters.
    /// </summary>
    /// <param name="identifier">The identifier to escape.</param>
    /// <returns>The escaped identifier, prefixed with '@' if needed.</returns>
    /// <exception cref="ArgumentNullException">Thrown when identifier is null.</exception>
    public static string EscapeIdentifier(string identifier)
    {
        ArgumentNullException.ThrowIfNull(identifier);

        var keywords = new HashSet<string>(StringComparer.Ordinal)
        {
            "abstract", "as", "base", "bool", "break", "byte", "case", "catch", "char", "checked",
            "class", "const", "continue", "decimal", "default", "delegate", "do", "double", "else", "enum",
            "event", "explicit", "extern", "false", "finally", "fixed", "float", "for", "foreach", "goto",
            "if", "implicit", "in", "int", "interface", "internal", "is", "lock", "long", "namespace",
            "new", "null", "object", "operator", "out", "override", "params", "private", "protected", "public",
            "readonly", "ref", "return", "sbyte", "sealed", "short", "sizeof", "stackalloc", "static", "string",
            "struct", "switch", "this", "throw", "true", "try", "typeof", "uint", "ulong", "unchecked",
            "unsafe", "ushort", "using", "virtual", "void", "volatile", "while"
        };

        if (keywords.Contains(identifier))
        {
            return "@" + identifier;
        }

        if (identifier.Length > 0 && char.IsDigit(identifier[0]))
        {
            return "@" + identifier;
        }

        if (identifier.Length == 0 ||
            (!char.IsLetter(identifier[0]) && identifier[0] != '_'))
        {
            return "@" + identifier;
        }

        foreach (var c in identifier)
        {
            if (!char.IsLetterOrDigit(c) && c != '_')
            {
                return "@" + identifier;
            }
        }

        return identifier;
    }

    /// <summary>
    /// Formats a type name using Roslyn's SymbolDisplayFormat for proper
    /// handling of generics, nested types, and global namespace types.
    /// </summary>
    /// <param name="typeName">The type name to format.</param>
    /// <returns>Properly formatted type name.</returns>
    /// <exception cref="ArgumentNullException">Thrown when typeName is null.</exception>
    public static string FormatTypeName(string typeName)
    {
        ArgumentNullException.ThrowIfNull(typeName);

        var normalizedType = typeName.Trim();

        var typeAliasMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "int", "int" },
            { "long", "long" },
            { "decimal", "decimal" },
            { "double", "double" },
            { "float", "float" },
            { "bool", "bool" },
            { "string", "string" },
            { "object", "object" },
            { "void", "void" },
            { "byte", "byte" },
            { "sbyte", "sbyte" },
            { "short", "short" },
            { "ushort", "ushort" },
            { "uint", "uint" },
            { "ulong", "ulong" },
            { "char", "char" },
            { "nint", "nint" },
            { "nuint", "nuint" }
        };

        if (typeAliasMap.TryGetValue(normalizedType, out var fullName))
        {
            return fullName;
        }

        try
        {
            var typeSyntax = SyntaxFactory.ParseTypeName(normalizedType);
            var formattedTypeName = typeSyntax.ToFullString();
            return CleanRoslynFormattedTypeName(formattedTypeName);
        }
        catch
        {
            return FormatTypeNameFallback(normalizedType);
        }
    }

    /// <summary>
    /// Fallback method for formatting type names when Roslyn parsing fails.
    /// </summary>
    /// <param name="typeName">The type name to format.</param>
    /// <returns>Properly formatted type name.</returns>
    private static string FormatTypeNameFallback(string typeName)
    {
        var normalizedType = typeName.Trim();

        if (normalizedType.Contains('`'))
        {
            var parts = normalizedType.Split('`');
            if (parts.Length == 2 && int.TryParse(parts[1], out var arity))
            {
                var genericParams = string.Join(", ", Enumerable.Range(1, arity).Select(i => $"T{i}"));
                return parts[0] + "<" + genericParams + ">";
            }
        }

        if (normalizedType.Contains('+'))
        {
            return normalizedType.Replace('+', '.');
        }

        if (normalizedType.StartsWith("::", StringComparison.Ordinal))
        {
            return "global" + normalizedType;
        }

        return normalizedType;
    }

    /// <summary>
    /// Cleans up type names formatted by Roslyn's ToFullString() method.
    /// </summary>
    /// <param name="formattedTypeName">The type name formatted by Roslyn.</param>
    /// <returns>Cleaned up type name.</returns>
    private static string CleanRoslynFormattedTypeName(string formattedTypeName)
    {
        var cleaned = formattedTypeName
            .Replace("  ", " ")
            .Trim();

        if (cleaned.EndsWith(";", StringComparison.Ordinal))
        {
            cleaned = cleaned.Substring(0, cleaned.Length - 1).Trim();
        }

        return cleaned;
    }

    /// <summary>
    /// Formats a type name with proper escaping for use as an identifier.
    /// </summary>
    /// <param name="typeName">The type name to format.</param>
    /// <returns>Properly formatted and escaped type name.</returns>
    public static string FormatTypeNameForIdentifier(string typeName)
    {
        ArgumentNullException.ThrowIfNull(typeName);
        var formatted = FormatTypeName(typeName);
        return EscapeIdentifier(formatted);
    }

    /// <summary>
    /// Escapes all identifiers in a type name string.
    /// </summary>
    /// <param name="typeName">The type name to process.</param>
    /// <returns>Type name with identifiers properly escaped.</returns>
    public static string EscapeTypeNameIdentifiers(string typeName)
    {
        ArgumentNullException.ThrowIfNull(typeName);

        var result = typeName;
        var openBracket = result.IndexOf('<');
        var closeBracket = result.LastIndexOf('>');

        if (openBracket >= 0 && closeBracket > openBracket)
        {
            var genericPart = result.Substring(openBracket + 1, closeBracket - openBracket - 1);
            var parts = genericPart.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < parts.Length; i++)
            {
                var part = parts[i].Trim();
                parts[i] = EscapeIdentifier(part);
            }

            var innerGeneric = string.Join(", ", parts);
            result = result.Substring(0, openBracket + 1) + innerGeneric + result.Substring(closeBracket);
        }

        return result;
    }

    /// <summary>
    /// Generates a complete source file with all generation options applied.
    /// </summary>
    /// <param name="options">Generation options controlling file format.</param>
    /// <param name="namespace">The target namespace.</param>
    /// <param name="code">The main code content.</param>
    /// <param name="additionalUsings">Additional using directives.</param>
    /// <param name="author">Author information.</param>
    /// <param name="description">File description.</param>
    /// <param name="generatorName">Name of the generator producing the code.</param>
    /// <param name="generatorVersion">Version of the generator.</param>
    /// <returns>Complete formatted source file.</returns>
    public static string GenerateSourceFile(
        GenerationOptions options,
        string @namespace,
        string code,
        string[] additionalUsings = null,
        string author = "Vladyslav Zaiets | https://sarmkadan.com",
        string description = null,
        string generatorName = "DotNetSourceGeneratorToolkit",
        string generatorVersion = "1.0.0")
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentException.ThrowIfNullOrEmpty(@namespace);

        var lines = new List<string>();

        // Add language version directive if specified
        var langVersionDirective = options.GetLangVersionDirective();
        if (!string.IsNullOrEmpty(langVersionDirective))
        {
            lines.Add(langVersionDirective);
        }

        // Add nullable directive
        var nullableDirective = options.GetNullableDirective();
        if (!string.IsNullOrEmpty(nullableDirective))
        {
            lines.Add(nullableDirective);
        }

        // Add header comment
        var header = options.GetHeaderComment(author, description);
        if (!string.IsNullOrEmpty(header))
        {
            lines.Add(header);
        }

        // Add [GeneratedCode] attribute if enabled
        var generatedCodeAttribute = options.GetGeneratedCodeAttribute(generatorName, generatorVersion);
        if (!string.IsNullOrEmpty(generatedCodeAttribute))
        {
            lines.Add(generatedCodeAttribute);
        }

        // Add usings
        var usings = GenerateUsings(additionalUsings);
        lines.Add(usings);
        lines.Add(string.Empty);

        // Add namespace wrapper
        var namespacedCode = WrapInNamespace(options, @namespace, code);
        lines.Add(namespacedCode);

        return string.Join(Environment.NewLine, lines);
    }
}