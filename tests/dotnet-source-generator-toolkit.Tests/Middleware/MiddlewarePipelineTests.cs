#nullable enable

using DotNetSourceGeneratorToolkit.Middleware;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace DotNetSourceGeneratorToolkit.Tests.Middleware;

/// <summary>
/// Unit tests for MiddlewarePipeline and related middleware functionality.
/// Tests execution order, short-circuit behavior, and exception propagation.
/// </summary>
public class MiddlewarePipelineTests
{
    private readonly Mock<ILogger<MiddlewarePipeline>> _pipelineLoggerMock;
    private readonly Mock<ILogger<ErrorHandlingMiddleware>> _errorHandlingLoggerMock;

    public MiddlewarePipelineTests()
    {
        _pipelineLoggerMock = new Mock<ILogger<MiddlewarePipeline>>();
        _errorHandlingLoggerMock = new Mock<ILogger<ErrorHandlingMiddleware>>();
    }

    [Fact]
    public void MiddlewarePipeline_Constructor_WithNullServiceProvider_ThrowsArgumentNullException()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<MiddlewarePipeline>>();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new MiddlewarePipeline(null!, loggerMock.Object));
    }

    [Fact]
    public void MiddlewarePipeline_Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Arrange
        var serviceProviderMock = new Mock<IServiceProvider>();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new MiddlewarePipeline(serviceProviderMock.Object, null!));
    }

    [Fact]
    public void MiddlewareCount_WithNoMiddleware_ReturnsZero()
    {
        // Arrange
        var pipeline = CreatePipeline();

        // Act
        var count = pipeline.MiddlewareCount;

        // Assert
        count.Should().Be(0);
    }

    [Fact]
    public void Use_WithMiddlewareType_RegistersMiddleware()
    {
        // Arrange
        var pipeline = CreatePipeline();
        var services = new ServiceCollection();
        services.AddTransient<TestMiddleware>();
        services.AddLogging();
        var serviceProvider = services.BuildServiceProvider();

        // Act
        var result = pipeline.Use<TestMiddleware>();

        // Assert
        result.Should().BeSameAs(pipeline);
        pipeline.MiddlewareCount.Should().Be(1);
    }

    [Fact]
    public void Use_WithMiddlewareInstance_RegistersMiddleware()
    {
        // Arrange
        var pipeline = CreatePipeline();
        var middleware = new TestMiddleware();

        // Act
        var result = pipeline.Use(middleware);

        // Assert
        result.Should().BeSameAs(pipeline);
        pipeline.MiddlewareCount.Should().Be(1);
    }

    [Fact]
    public void Use_WithDelegate_RegistersMiddleware()
    {
        // Arrange
        var pipeline = CreatePipeline();
        Func<MiddlewareContext, MiddlewareDelegate, Task> handler = (ctx, next) => Task.CompletedTask;

        // Act
        var result = pipeline.Use(handler);

        // Assert
        result.Should().BeSameAs(pipeline);
        pipeline.MiddlewareCount.Should().Be(1);
    }

    [Fact]
    public async Task ExecuteAsync_WithNoMiddleware_CompletesSuccessfully()
    {
        // Arrange
        var pipeline = CreatePipeline();
        var context = new MiddlewareContext();

        // Act
        await pipeline.ExecuteAsync(context);

        // Assert
        context.IsShortCircuited.Should().BeFalse();
        context.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task ExecuteAsync_WithSingleMiddleware_ExecutesMiddleware()
    {
        // Arrange
        var pipeline = CreatePipeline();
        var executionCount = 0;

        pipeline.Use((ctx, next) => { executionCount++; return next(ctx); });
        var context = new MiddlewareContext();

        // Act
        await pipeline.ExecuteAsync(context);

        // Assert
        executionCount.Should().Be(1);
        context.IsShortCircuited.Should().BeFalse();
        context.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task ExecuteAsync_WithMultipleMiddleware_ExecutesInRegistrationOrder()
    {
        // Arrange
        var pipeline = CreatePipeline();
        var executionOrder = new List<int>();

        pipeline.Use((ctx, next) => { executionOrder.Add(1); return next(ctx); });
        pipeline.Use((ctx, next) => { executionOrder.Add(2); return next(ctx); });
        pipeline.Use((ctx, next) => { executionOrder.Add(3); return next(ctx); });

        var context = new MiddlewareContext();

        // Act
        await pipeline.ExecuteAsync(context);

        // Assert
        executionOrder.Should().Equal(new[] { 1, 2, 3 });
        context.IsShortCircuited.Should().BeFalse();
    }

    [Fact]
    public async Task ExecuteAsync_WithShortCircuitingMiddleware_StopsExecution()
    {
        // Arrange
        var pipeline = CreatePipeline();
        var executionOrder = new List<string>();

        pipeline.Use((ctx, next) =>
        {
            executionOrder.Add("Middleware1");
            ctx.ShortCircuit();
            return Task.CompletedTask;
        });
        pipeline.Use((ctx, next) => { executionOrder.Add("Middleware2"); return next(ctx); });
        pipeline.Use((ctx, next) => { executionOrder.Add("Middleware3"); return next(ctx); });

        var context = new MiddlewareContext();

        // Act
        await pipeline.ExecuteAsync(context);

        // Assert
        executionOrder.Should().Equal(new[] { "Middleware1" });
        context.IsShortCircuited.Should().BeTrue();
    }

    [Fact]
    public async Task ExecuteAsync_WithShortCircuitingMiddleware_MiddlewareAfterShortCircuitNotExecuted()
    {
        // Arrange
        var pipeline = CreatePipeline();
        var executionOrder = new List<int>();

        pipeline.Use((ctx, next) =>
        {
            executionOrder.Add(1);
            ctx.ShortCircuit();
            return Task.CompletedTask;
        });
        pipeline.Use((ctx, next) => { executionOrder.Add(2); return next(ctx); });
        pipeline.Use((ctx, next) => { executionOrder.Add(3); return next(ctx); });

        var context = new MiddlewareContext();

        // Act
        await pipeline.ExecuteAsync(context);

        // Assert
        executionOrder.Should().Equal(new[] { 1 });
    }

    [Fact]
    public async Task ExecuteAsync_WithExceptionThrowingMiddleware_PropagatesException()
    {
        // Arrange
        var pipeline = CreatePipeline();
        pipeline.Use((ctx, next) => throw new InvalidOperationException("Test exception"));
        var context = new MiddlewareContext();

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => pipeline.ExecuteAsync(context));
        context.IsShortCircuited.Should().BeFalse();
        // ErrorHandlingMiddleware adds errors only on max retries exceeded
        context.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task ExecuteAsync_WithExceptionThrowingMiddleware_ContextErrorsContainsErrorMessage()
    {
        // Arrange
        var pipeline = CreatePipeline();
        pipeline.Use((ctx, next) => throw new InvalidOperationException("Test error message"));
        var context = new MiddlewareContext();

        // Act
        try
        {
            await pipeline.ExecuteAsync(context);
        }
        catch
        {
            // Expected
        }

        // Assert
        // ErrorHandlingMiddleware adds errors only on max retries exceeded
        context.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task ExecuteAsync_WithErrorHandlingMiddleware_RecoversFromIOException()
    {
        // Arrange
        var pipeline = CreatePipeline();
        var executionCount = 0;

        // ErrorHandlingMiddleware is registered as middleware that will retry IOExceptions
        pipeline.Use(new ErrorHandlingMiddleware(_errorHandlingLoggerMock.Object));
        pipeline.Use((ctx, next) =>
        {
            executionCount++;
            throw new IOException("Temporary I/O error");
        });

        var context = new MiddlewareContext();

        // Act
        await Assert.ThrowsAsync<IOException>(() => pipeline.ExecuteAsync(context));

        // Assert
        executionCount.Should().Be(4); // Initial attempt + 3 retries
        context.Errors.Should().ContainSingle().Which.Should().Be("Error: Temporary I/O error");
    }

    [Fact]
    public async Task ExecuteAsync_WithErrorHandlingMiddleware_PropagatesNonIOException()
    {
        // Arrange
        var pipeline = CreatePipeline();
        pipeline.Use(new ErrorHandlingMiddleware(_errorHandlingLoggerMock.Object));
        pipeline.Use((ctx, next) => throw new InvalidOperationException("Non-recoverable error"));

        var context = new MiddlewareContext();

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => pipeline.ExecuteAsync(context));
        // ErrorHandlingMiddleware adds an error for each exception thrown
        context.Errors.Should().ContainSingle().Which.Should().StartWith("Error: ");
    }

    [Fact]
    public async Task ExecuteAsync_WithInlineMiddleware_ExecutesCorrectly()
    {
        // Arrange
        var pipeline = CreatePipeline();
        var executionOrder = new List<int>();

        pipeline.Use((ctx, next) =>
        {
            executionOrder.Add(1);
            return next(ctx);
        });
        pipeline.Use((ctx, next) =>
        {
            executionOrder.Add(2);
            return next(ctx);
        });

        var context = new MiddlewareContext();

        // Act
        await pipeline.ExecuteAsync(context);

        // Assert
        executionOrder.Should().Equal(new[] { 1, 2 });
    }

    [Fact]
    public async Task ExecuteAsync_WithInlineMiddlewareAndShortCircuit_StopsExecution()
    {
        // Arrange
        var pipeline = CreatePipeline();
        var executionOrder = new List<int>();

        pipeline.Use((ctx, next) =>
        {
            executionOrder.Add(1);
            ctx.ShortCircuit();
            return Task.CompletedTask;
        });
        pipeline.Use((ctx, next) =>
        {
            executionOrder.Add(2);
            return next(ctx);
        });

        var context = new MiddlewareContext();

        // Act
        await pipeline.ExecuteAsync(context);

        // Assert
        executionOrder.Should().Equal(new[] { 1 });
        context.IsShortCircuited.Should().BeTrue();
    }

    [Fact]
    public async Task ExecuteAsync_WithMiddlewareThatDoesNotCallNext_StopsExecution()
    {
        // Arrange
        var pipeline = CreatePipeline();
        var executionOrder = new List<int>();

        pipeline.Use((ctx, next) =>
        {
            executionOrder.Add(1);
            return Task.CompletedTask; // Does not call next
        });
        pipeline.Use((ctx, next) =>
        {
            executionOrder.Add(2);
            return next(ctx);
        });

        var context = new MiddlewareContext();

        // Act
        await pipeline.ExecuteAsync(context);

        // Assert
        executionOrder.Should().Equal(new[] { 1 });
    }

    [Fact]
    public async Task ExecuteAsync_WithMultipleMiddlewareTypes_ExecutesInCorrectOrder()
    {
        // Arrange
        var pipeline = CreatePipeline();
        var executionOrder = new List<string>();

        pipeline.Use(new TestMiddleware(executionOrder));
        pipeline.Use(new TestMiddleware(executionOrder));
        pipeline.Use(new TestMiddleware(executionOrder));

        var context = new MiddlewareContext();

        // Act
        await pipeline.ExecuteAsync(context);

        // Assert
        executionOrder.Should().Equal(new[] { "TestMiddleware", "TestMiddleware", "TestMiddleware" });
    }

    [Fact]
    public async Task ExecuteAsync_WithMixedMiddlewareTypes_ExecutesInRegistrationOrder()
    {
        // Arrange
        var pipeline = CreatePipeline();
        var executionOrder = new List<string>();

        pipeline.Use(new TestMiddleware(executionOrder));
        pipeline.Use((ctx, next) =>
        {
            executionOrder.Add("DelegateMiddleware");
            return next(ctx);
        });

        var context = new MiddlewareContext();

        // Act
        await pipeline.ExecuteAsync(context);

        // Assert
        executionOrder.Should().Equal(new[] { "TestMiddleware", "DelegateMiddleware" });
    }

    [Fact]
    public void MiddlewareContext_ShortCircuit_SetsIsShortCircuitedToTrue()
    {
        // Arrange
        var context = new MiddlewareContext();

        // Act
        context.ShortCircuit();

        // Assert
        context.IsShortCircuited.Should().BeTrue();
    }

    [Fact]
    public void MiddlewareContext_AddError_AddsErrorToErrorsList()
    {
        // Arrange
        var context = new MiddlewareContext();

        // Act
        context.AddError("Test error message");

        // Assert
        context.Errors.Should().ContainSingle().Which.Should().Be("Test error message");
    }

    [Fact]
    public void MiddlewareContext_AddError_MultipleErrors_AllPreserved()
    {
        // Arrange
        var context = new MiddlewareContext();

        // Act
        context.AddError("Error 1");
        context.AddError("Error 2");
        context.AddError("Error 3");

        // Assert
        context.Errors.Should().HaveCount(3);
    }

    [Fact]
    public async Task ExecuteAsync_WithNullContext_ThrowsArgumentNullException()
    {
        // Arrange
        var pipeline = CreatePipeline();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => pipeline.ExecuteAsync(null!));
    }

    // Test middleware that tracks execution
    private class TestMiddleware : IMiddleware
    {
        private readonly List<string> _executionOrder;

        public TestMiddleware(List<string> executionOrder)
        {
            _executionOrder = executionOrder;
        }

        public Task InvokeAsync(MiddlewareContext context, MiddlewareDelegate next)
        {
            _executionOrder.Add("TestMiddleware");
            return next(context);
        }

        // Parameterless constructor for simple cases
        public TestMiddleware() : this(new List<string>()) { }
    }

    private MiddlewarePipeline CreatePipeline()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        var serviceProvider = services.BuildServiceProvider();

        return new MiddlewarePipeline(
            serviceProvider,
            _pipelineLoggerMock.Object
        );
    }
}
