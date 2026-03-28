// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.Extensions.DependencyInjection;

namespace DotNetSourceGeneratorToolkit.Middleware;

/// <summary>
/// Implements a composable middleware pipeline for request processing.
/// Builds a chain of responsibility where each middleware can process and pass control.
/// </summary>
public class MiddlewarePipeline : IMiddlewarePipeline
{
    private readonly List<MiddlewareFactory> _middlewares = new();
    private readonly IServiceProvider _serviceProvider;

    public int MiddlewareCount => _middlewares.Count;

    public MiddlewarePipeline(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    public IMiddlewarePipeline Use<TMiddleware>() where TMiddleware : IMiddleware
    {
        _middlewares.Add(context =>
        {
            var middleware = _serviceProvider.GetRequiredService<TMiddleware>();
            return middleware;
        });

        return this;
    }

    public IMiddlewarePipeline Use(IMiddleware middleware)
    {
        _middlewares.Add(_ => middleware);
        return this;
    }

    public IMiddlewarePipeline Use(Func<MiddlewareContext, MiddlewareDelegate, Task> handler)
    {
        // Wrap inline handler as IMiddleware
        var middleware = new DelegateMiddleware(handler);
        _middlewares.Add(_ => middleware);
        return this;
    }

    public async Task ExecuteAsync(MiddlewareContext context)
    {
        if (context == null)
            throw new ArgumentNullException(nameof(context));

        // Build the chain of responsibility
        MiddlewareDelegate next = _ => Task.CompletedTask; // Terminal middleware

        for (int i = _middlewares.Count - 1; i >= 0; i--)
        {
            var currentNext = next;
            var middlewareFactory = _middlewares[i];
            var middleware = middlewareFactory(context);

            next = ctx =>
            {
                if (ctx.IsShortCircuited)
                    return Task.CompletedTask;

                return middleware.InvokeAsync(ctx, currentNext);
            };
        }

        await next(context);
    }

    // Factory delegate for creating middleware instances
    private delegate IMiddleware MiddlewareFactory(MiddlewareContext context);

    // Wrapper for inline middleware delegates
    private class DelegateMiddleware : IMiddleware
    {
        private readonly Func<MiddlewareContext, MiddlewareDelegate, Task> _handler;

        public DelegateMiddleware(Func<MiddlewareContext, MiddlewareDelegate, Task> handler)
        {
            _handler = handler;
        }

        public Task InvokeAsync(MiddlewareContext context, MiddlewareDelegate next)
        {
            return _handler(context, next);
        }
    }
}
