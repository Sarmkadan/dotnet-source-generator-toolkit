// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.Extensions.Logging;

namespace DotNetSourceGeneratorToolkit.Middleware;

/// <summary>
/// Middleware that validates CLI options and project structure before generation.
/// Prevents invalid configurations from reaching generators and wasting resources.
/// </summary>
public class ValidationMiddleware : IMiddleware
{
    private readonly ILogger<ValidationMiddleware> _logger;
    private readonly CLI.ICliArgumentParser _argumentParser;

    public ValidationMiddleware(
        ILogger<ValidationMiddleware> logger,
        CLI.ICliArgumentParser argumentParser)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _argumentParser = argumentParser ?? throw new ArgumentNullException(nameof(argumentParser));
    }

    public async Task InvokeAsync(MiddlewareContext context, MiddlewareDelegate next)
    {
        var requestId = context.RequestId;

        _logger.LogInformation("[{RequestId}] Starting validation phase", requestId);

        // Validate CLI options
        if (context.CliOptions == null)
        {
            var msg = "CLI options are not set in context";
            _logger.LogError("[{RequestId}] {Message}", requestId, msg);
            context.AddError(msg);
            context.ShortCircuit();
            return;
        }

        var optionErrors = _argumentParser.Validate(context.CliOptions).ToList();
        if (optionErrors.Count > 0)
        {
            _logger.LogError("[{RequestId}] CLI validation failed with {Count} errors", requestId, optionErrors.Count);
            foreach (var error in optionErrors)
            {
                context.AddError(error);
            }

            context.ShortCircuit();
            return;
        }

        _logger.LogInformation("[{RequestId}] CLI options validation passed", requestId);

        // Early exit for validation-only mode
        if (context.CliOptions.ValidateOnly)
        {
            _logger.LogInformation("[{RequestId}] Validation-only mode: skipping generation", requestId);
            context.ShortCircuit();
            return;
        }

        await next(context);

        // Post-execution validation
        if (context.GenerationResults.Count == 0 && !context.CliOptions.DryRun)
        {
            _logger.LogWarning("[{RequestId}] No generation results produced", requestId);
        }
    }
}
