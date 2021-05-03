namespace DotNetSourceGeneratorToolkit.Middleware;

public static class MiddlewarePipelineExtensions
{
    /// <summary>
    /// Adds a middleware to the pipeline if the specified condition is true.
    /// </summary>
    /// <param name="pipeline">The middleware pipeline.</param>
    /// <param name="condition">The condition to check.</param>
    /// <param name="middleware">The middleware to add.</param>
    /// <returns>The middleware pipeline.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="pipeline"/> or <paramref name="middleware"/> is null.</exception>
    public static IMiddlewarePipeline UseIf(this IMiddlewarePipeline pipeline, bool condition, IMiddleware middleware)
    {
        ArgumentNullException.ThrowIfNull(pipeline);
        ArgumentNullException.ThrowIfNull(middleware);

        if (condition)
        {
            pipeline.Use(middleware);
        }

        return pipeline;
    }

    /// <summary>
    /// Runs the middleware pipeline and returns the result as a task.
    /// </summary>
    /// <param name="pipeline">The middleware pipeline.</param>
    /// <param name="context">The middleware context.</param>
    /// <returns>A task representing the result of running the pipeline.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="pipeline"/> or <paramref name="context"/> is null.</exception>
    public static async Task RunAsync(this MiddlewarePipeline pipeline, MiddlewareContext context)
    {
        ArgumentNullException.ThrowIfNull(pipeline);
        ArgumentNullException.ThrowIfNull(context);

        await pipeline.ExecuteAsync(context);
    }
}
