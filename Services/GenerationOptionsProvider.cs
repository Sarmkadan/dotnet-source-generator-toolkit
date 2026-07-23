#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using DotNetSourceGeneratorToolkit.Domain;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.Extensions.Logging;

namespace DotNetSourceGeneratorToolkit.Services;

/// <summary>
/// Provides generation options by reading MSBuild properties via AnalyzerConfigOptionsProvider.
/// </summary>
public interface IGenerationOptionsProvider
{
    /// <summary>
    /// Gets the generation options for the current compilation context.
    /// </summary>
    /// <returns>Configured generation options.</returns>
    GenerationOptions GetGenerationOptions();

    /// <summary>
    /// Gets the generation options with the specified overrides.
    /// </summary>
    /// <param name="overrides">Options to override the defaults.</param>
    /// <returns>Merged generation options.</returns>
    GenerationOptions GetGenerationOptions(GenerationOptions overrides);
}

/// <summary>
/// Default implementation of GenerationOptionsProvider that reads from MSBuild properties.
/// </summary>
public sealed class GenerationOptionsProvider : IGenerationOptionsProvider
{
    private readonly ILogger<GenerationOptionsProvider> _logger;
    private readonly AnalyzerConfigOptionsProvider _configOptionsProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="GenerationOptionsProvider"/> class.
    /// </summary>
    /// <param name="configOptionsProvider">MSBuild analyzer config options provider.</param>
    /// <param name="logger">Logger instance.</param>
    public GenerationOptionsProvider(
        AnalyzerConfigOptionsProvider configOptionsProvider,
        ILogger<GenerationOptionsProvider> logger)
    {
        _configOptionsProvider = configOptionsProvider ?? throw new ArgumentNullException(nameof(configOptionsProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public GenerationOptions GetGenerationOptions()
    {
        _logger.LogDebug("Reading generation options from MSBuild configuration");

        var options = new GenerationOptions();
        var globalOptions = _configOptionsProvider.GlobalOptions;

        try
        {
            // Read nullable context option
            if (globalOptions.TryGetValue("dotnet_diagnostic.SGTK001.preferred_nullable_context", out var nullableContextStr))
            {
                if (Enum.TryParse<NullableContext>(nullableContextStr, true, out var nullableContext))
                {
                    options = options with { NullableContext = nullableContext };
                    _logger.LogInformation("Configured nullable context: {NullableContext}", nullableContext);
                }
            }

            // Read namespace style option
            if (globalOptions.TryGetValue("dotnet_diagnostic.SGTK002.preferred_namespace_style", out var namespaceStyleStr))
            {
                if (Enum.TryParse<NamespaceStyle>(namespaceStyleStr, true, out var namespaceStyle))
                {
                    options = options with { NamespaceStyle = namespaceStyle };
                    _logger.LogInformation("Configured namespace style: {NamespaceStyle}", namespaceStyle);
                }
            }

            // Read generated code attribute mode
            if (globalOptions.TryGetValue("dotnet_diagnostic.SGTK003.preferred_generated_code_attribute", out var attributeModeStr))
            {
                if (Enum.TryParse<GeneratedCodeAttributeMode>(attributeModeStr, true, out var attributeMode))
                {
                    options = options with { EmitGeneratedCodeAttribute = attributeMode };
                    _logger.LogInformation("Configured generated code attribute mode: {AttributeMode}", attributeMode);
                }
            }

            // Read custom header comment
            if (globalOptions.TryGetValue("dotnet_diagnostic.SGTK004.preferred_header_comment", out var headerComment))
            {
                options = options with { HeaderComment = headerComment };
                _logger.LogInformation("Configured custom header comment");
            }

            // Read target language version
            if (globalOptions.TryGetValue("build_property.LangVersion", out var langVersion))
            {
                options = options with { LangVersion = langVersion };
                _logger.LogInformation("Configured language version: {LangVersion}", langVersion);
            }

            // Read indentation size
            if (globalOptions.TryGetValue("dotnet_diagnostic.SGTK005.preferred_indent_size", out var indentSizeStr) &&
                int.TryParse(indentSizeStr, out var indentSize) &&
                indentSize > 0)
            {
                options = options with { IndentSize = indentSize };
                _logger.LogInformation("Configured indent size: {IndentSize}", indentSize);
            }

            options.Validate();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error reading generation options from MSBuild configuration");
            // Return default options if there's an error
        }

        _logger.LogInformation("Using generation options: {Options}", options);
        return options;
    }

    /// <inheritdoc />
    public GenerationOptions GetGenerationOptions(GenerationOptions overrides)
    {
        var baseOptions = GetGenerationOptions();
        return baseOptions with {
            NullableContext = overrides.NullableContext,
            NamespaceStyle = overrides.NamespaceStyle,
            EmitGeneratedCodeAttribute = overrides.EmitGeneratedCodeAttribute,
            HeaderComment = overrides.HeaderComment ?? baseOptions.HeaderComment,
            LangVersion = overrides.LangVersion ?? baseOptions.LangVersion,
            IndentSize = overrides.IndentSize
        };
    }
}