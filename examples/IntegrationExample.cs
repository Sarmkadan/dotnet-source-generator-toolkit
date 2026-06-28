// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNetSourceGeneratorToolkit.Configuration;
using DotNetSourceGeneratorToolkit.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace DotNetSourceGeneratorToolkit.Examples;

/// <summary>
/// Example showing how to wire the toolkit into ASP.NET Core DI.
/// </summary>
public sealed class IntegrationExample
{
    public void ConfigureServices(IServiceCollection services)
    {
        // Define configuration
        var options = new ToolkitOptions
        {
            DefaultNamespace = "Generated.Code"
        };

        // Register the toolkit in the dependency injection container
        services.AddSourceGeneratorToolkit(options);

        // Now you can inject ISourceGeneratorService, IRepositoryGeneratorService, etc.
        // into your controllers or background services.
    }
}
