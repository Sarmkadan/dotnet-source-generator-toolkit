// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNetSourceGeneratorToolkit.Configuration;
using DotNetSourceGeneratorToolkit.Domain;
using DotNetSourceGeneratorToolkit.Exceptions;
using DotNetSourceGeneratorToolkit.Services;

namespace DotNetSourceGeneratorToolkit.Examples;

/// <summary>
/// An advanced example demonstrating custom options and error handling.
/// </summary>
public sealed class AdvancedUsage
{
    public async Task RunAdvancedGenerationAsync(ISourceGeneratorService generatorService)
    {
        // Define custom options
        var options = new ToolkitOptions
        {
            DefaultNamespace = "MyProject.Generated",
            ValidateTemplates = true,
            EnableCaching = true
        };

        try
        {
            // Execute generation with custom options
            var result = await generatorService.GenerateAsync(options);

            if (result.Status == GenerationStatus.Success)
            {
                Console.WriteLine("Generation completed successfully.");
            }
            else
            {
                Console.WriteLine($"Generation failed: {result.ErrorMessage}");
            }
        }
        catch (GenerationException ex)
        {
            // Handle generation-specific exceptions
            Console.WriteLine($"Critical generation error: {ex.Message}");
        }
    }
}
