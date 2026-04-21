// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetSourceGeneratorToolkit.Middleware;

/// <summary>
/// Contract for middleware that processes the generation pipeline.
/// Each middleware can inspect, modify, or short-circuit the pipeline.
/// </summary>
public interface IMiddleware
{
    /// <summary>
    /// Execute the middleware logic and pass control to the next middleware.
    /// </summary>
    /// <param name="context">Pipeline context containing request state</param>
    /// <param name="next">Delegate to invoke the next middleware in the chain</param>
    /// <returns>Awaitable task representing middleware execution</returns>
    Task InvokeAsync(MiddlewareContext context, MiddlewareDelegate next);
}

/// <summary>
/// Delegate for invoking the next middleware in the pipeline.
/// </summary>
public delegate Task MiddlewareDelegate(MiddlewareContext context);

/// <summary>
/// Encapsulates context information flowing through the middleware pipeline.
/// </summary>
public class MiddlewareContext
{
    /// <summary>
    /// Unique request identifier for tracing.
    /// </summary>
    public string RequestId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Original CLI options from argument parsing.
    /// </summary>
    public CLI.CliOptions? CliOptions { get; set; }

    /// <summary>
    /// Project information discovered during analysis.
    /// </summary>
    public Domain.ProjectInfo? ProjectInfo { get; set; }

    /// <summary>
    /// Generation results accumulated during processing.
    /// </summary>
    public List<Domain.GenerationResult> GenerationResults { get; set; } = new();

    /// <summary>
    /// Errors encountered during pipeline execution.
    /// </summary>
    public List<string> Errors { get; set; } = new();

    /// <summary>
    /// Execution timestamp in UTC.
    /// </summary>
    public DateTime StartTime { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Flag to indicate pipeline should stop processing.
    /// </summary>
    public bool IsShortCircuited { get; set; }

    /// <summary>
    /// Add an error to the context without stopping execution.
    /// </summary>
    public void AddError(string message) => Errors.Add(message);

    /// <summary>
    /// Signal that pipeline processing should stop.
    /// </summary>
    public void ShortCircuit() => IsShortCircuited = true;
}
