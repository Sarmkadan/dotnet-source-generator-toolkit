// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetSourceGeneratorToolkit.Middleware;

/// <summary>
/// Manages the composition and execution of middleware in the generation pipeline.
/// Supports fluent API for middleware registration and execution.
/// </summary>
public interface IMiddlewarePipeline
{
    /// <summary>
    /// Register a middleware type to be instantiated and executed in order.
    /// </summary>
    /// <typeparam name="TMiddleware">Middleware type to register</typeparam>
    /// <returns>Pipeline instance for method chaining</returns>
    IMiddlewarePipeline Use<TMiddleware>() where TMiddleware : IMiddleware;

    /// <summary>
    /// Register a middleware instance directly.
    /// </summary>
    /// <param name="middleware">Middleware instance to register</param>
    /// <returns>Pipeline instance for method chaining</returns>
    IMiddlewarePipeline Use(IMiddleware middleware);

    /// <summary>
    /// Register an inline middleware using a delegate.
    /// </summary>
    /// <param name="handler">Async delegate that implements middleware logic</param>
    /// <returns>Pipeline instance for method chaining</returns>
    IMiddlewarePipeline Use(Func<MiddlewareContext, MiddlewareDelegate, Task> handler);

    /// <summary>
    /// Execute the assembled pipeline with given context.
    /// </summary>
    /// <param name="context">Pipeline context to process</param>
    /// <returns>Awaitable task representing pipeline execution</returns>
    Task ExecuteAsync(MiddlewareContext context);

    /// <summary>
    /// Get count of registered middleware in the pipeline.
    /// </summary>
    int MiddlewareCount { get; }
}
