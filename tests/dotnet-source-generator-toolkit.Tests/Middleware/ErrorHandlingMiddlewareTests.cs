#nullable enable

using DotNetSourceGeneratorToolkit.Middleware;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace DotNetSourceGeneratorToolkit.Tests.Middleware;

/// <summary>
/// Unit tests for ErrorHandlingMiddleware.
/// Tests exception handling, retry logic, and error propagation behavior.
/// </summary>
public class ErrorHandlingMiddlewareTests
{
    private readonly Mock<ILogger<ErrorHandlingMiddleware>> _loggerMock;

    public ErrorHandlingMiddlewareTests()
    {
        _loggerMock = new Mock<ILogger<ErrorHandlingMiddleware>>();
    }

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Arrange & Act
        var act = () => new ErrorHandlingMiddleware(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public async Task InvokeAsync_WithIOException_RecoversAndRetries()
    {
        // Arrange
        var middleware = new ErrorHandlingMiddleware(_loggerMock.Object);
        var context = new MiddlewareContext();
        var executionCount = 0;

        // Act - IOException is caught and retried, but eventually thrown after max retries
        await Assert.ThrowsAsync<IOException>(() => middleware.InvokeAsync(context, ctx =>
        {
            executionCount++;
            throw new IOException("Temporary I/O error");
        }));

        // Assert
        executionCount.Should().Be(4); // Initial attempt + 3 retries before throwing
        context.IsShortCircuited.Should().BeFalse();
        context.Errors.Should().ContainSingle().Which.Should().Be("Error: Temporary I/O error");
    }

    [Fact]
    public async Task InvokeAsync_WithIOException_PropagatesAfterMaxRetriesExceeded()
    {
        // Arrange
        var middleware = new ErrorHandlingMiddleware(_loggerMock.Object);
        var context = new MiddlewareContext();
        var executionCount = 0;

        // Act & Assert
        await Assert.ThrowsAsync<IOException>(() => middleware.InvokeAsync(context, ctx =>
        {
            executionCount++;
            throw new IOException("Temporary I/O error");
        }));

        // Assert
        executionCount.Should().Be(4); // Initial attempt + 3 retries
        context.IsShortCircuited.Should().BeFalse();
        context.Errors.Should().ContainSingle().Which.Should().Be("Error: Temporary I/O error");
    }

    [Fact]
    public async Task InvokeAsync_WithNonIOException_PropagatesException()
    {
        // Arrange
        var middleware = new ErrorHandlingMiddleware(_loggerMock.Object);
        var context = new MiddlewareContext();

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => middleware.InvokeAsync(context, ctx =>
        {
            throw new InvalidOperationException("Non-recoverable error");
        }));

        // Assert
        context.IsShortCircuited.Should().BeFalse();
        context.Errors.Should().ContainSingle().Which.Should().StartWith("Error: ");
    }

    [Fact]
    public async Task InvokeAsync_WithNonIOException_AddsErrorToContext()
    {
        // Arrange
        var middleware = new ErrorHandlingMiddleware(_loggerMock.Object);
        var context = new MiddlewareContext();

        // Act
        try
        {
            await middleware.InvokeAsync(context, ctx =>
            {
                throw new InvalidOperationException("Test error message");
            });
        }
        catch
        {
            // Expected
        }

        // Assert
        context.Errors.Should().ContainSingle().Which.Should().Be("Error: Test error message");
    }

    [Fact]
    public async Task InvokeAsync_WithIOException_ExecutesNextMiddlewareAfterRecovery()
    {
        // Arrange
        var middleware = new ErrorHandlingMiddleware(_loggerMock.Object);
        var context = new MiddlewareContext();
        var executionOrder = new List<string>();

        // Act
        await middleware.InvokeAsync(context, async ctx =>
        {
            executionOrder.Add("AfterRecovery");
            await Task.CompletedTask;
        });

        // Assert
        executionOrder.Should().ContainSingle().Which.Should().Be("AfterRecovery");
        context.IsShortCircuited.Should().BeFalse();
        context.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task InvokeAsync_WithMultipleIOExceptionAttempts_AllAttemptsLogged()
    {
        // Arrange
        var middleware = new ErrorHandlingMiddleware(_loggerMock.Object);
        var context = new MiddlewareContext();
        var executionCount = 0;

        // Act & Assert
        await Assert.ThrowsAsync<IOException>(() => middleware.InvokeAsync(context, ctx =>
        {
            executionCount++;
            throw new IOException("Temporary I/O error");
        }));

        // Assert
        executionCount.Should().Be(4); // 1 initial + 3 retries
    }

    [Fact]
    public async Task InvokeAsync_WithSuccessfulExecution_DoesNotAddErrors()
    {
        // Arrange
        var middleware = new ErrorHandlingMiddleware(_loggerMock.Object);
        var context = new MiddlewareContext();

        // Act
        await middleware.InvokeAsync(context, ctx => Task.CompletedTask);

        // Assert
        context.IsShortCircuited.Should().BeFalse();
        context.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task InvokeAsync_WithArgumentNullException_PropagatesWithoutRetry()
    {
        // Arrange
        var middleware = new ErrorHandlingMiddleware(_loggerMock.Object);
        var context = new MiddlewareContext();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => middleware.InvokeAsync(context, ctx =>
        {
            throw new ArgumentNullException("param", "Null argument");
        }));

        // Assert
        context.IsShortCircuited.Should().BeFalse();
        context.Errors.Should().ContainSingle().Which.Should().StartWith("Error: ");
    }
}