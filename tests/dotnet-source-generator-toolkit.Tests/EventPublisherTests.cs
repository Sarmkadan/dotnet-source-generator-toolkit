#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNetSourceGeneratorToolkit.Events;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace DotNetSourceGeneratorToolkit.Tests;

/// <summary>
/// Tests for the concrete IEventPublisher implementation (EventAggregator).
/// Tests cover: publish reaches subscribed handler, multiple handlers, handler for a different event type is not invoked,
/// and handler exceptions are handled gracefully.
/// </summary>
public sealed class EventPublisherTests
{
    private readonly Mock<ILogger<EventAggregator>> _mockLogger;
    private readonly IServiceProvider _serviceProvider;

    public EventPublisherTests()
    {
        _mockLogger = new Mock<ILogger<EventAggregator>>();
        var services = new ServiceCollection();
        services.AddSingleton(_mockLogger.Object);
        _serviceProvider = services.BuildServiceProvider();
    }

    [Fact]
    public async Task PublishAsync_WithSingleHandler_HandlerIsInvoked()
    {
        // Arrange
        var handlerMock = new Mock<IEventHandler<GenerationStartedEvent>>();
        handlerMock.Setup(h => h.HandleAsync(It.IsAny<GenerationStartedEvent>()))
                  .Returns(Task.CompletedTask);

        var services = new ServiceCollection();
        services.AddSingleton(handlerMock.Object);
        services.AddSingleton(_mockLogger.Object);
        var serviceProvider = services.BuildServiceProvider();

        var publisher = new EventAggregator(serviceProvider, _mockLogger.Object);
        var testEvent = new GenerationStartedEvent
        {
            RequestId = "test-request-1",
            ProjectPath = "/test/project.csproj",
            EntityCount = 42,
            GeneratorTypes = ["TestGenerator"]
        };

        // Act
        await publisher.PublishAsync(testEvent);

        // Assert
        handlerMock.Verify(h => h.HandleAsync(It.Is<GenerationStartedEvent>(e =>
            e.EventId == testEvent.EventId &&
            e.RequestId == testEvent.RequestId &&
            e.ProjectPath == testEvent.ProjectPath &&
            e.EntityCount == testEvent.EntityCount &&
            e.GeneratorTypes.SequenceEqual(testEvent.GeneratorTypes)
        )), Times.Once);
    }

    [Fact]
    public async Task PublishAsync_WithMultipleHandlers_AllHandlersAreInvoked()
    {
        // Arrange
        var handler1Mock = new Mock<IEventHandler<GenerationStartedEvent>>();
        handler1Mock.Setup(h => h.HandleAsync(It.IsAny<GenerationStartedEvent>()))
                   .Returns(Task.CompletedTask);

        var handler2Mock = new Mock<IEventHandler<GenerationStartedEvent>>();
        handler2Mock.Setup(h => h.HandleAsync(It.IsAny<GenerationStartedEvent>()))
                   .Returns(Task.CompletedTask);

        var handler3Mock = new Mock<IEventHandler<GenerationStartedEvent>>();
        handler3Mock.Setup(h => h.HandleAsync(It.IsAny<GenerationStartedEvent>()))
                   .Returns(Task.CompletedTask);

        var services = new ServiceCollection();
        services.AddSingleton(handler1Mock.Object);
        services.AddSingleton(handler2Mock.Object);
        services.AddSingleton(handler3Mock.Object);
        services.AddSingleton(_mockLogger.Object);
        var serviceProvider = services.BuildServiceProvider();

        var publisher = new EventAggregator(serviceProvider, _mockLogger.Object);
        var testEvent = new GenerationStartedEvent
        {
            RequestId = "test-request-2",
            ProjectPath = "/test/project2.csproj",
            EntityCount = 100,
            GeneratorTypes = ["GeneratorA", "GeneratorB"]
        };

        // Act
        await publisher.PublishAsync(testEvent);

        // Assert
        handler1Mock.Verify(h => h.HandleAsync(It.IsAny<GenerationStartedEvent>()), Times.Once);
        handler2Mock.Verify(h => h.HandleAsync(It.IsAny<GenerationStartedEvent>()), Times.Once);
        handler3Mock.Verify(h => h.HandleAsync(It.IsAny<GenerationStartedEvent>()), Times.Once);
    }

    [Fact]
    public async Task PublishAsync_WithMultipleEventTypes_OnlyMatchingHandlersAreInvoked()
    {
        // Arrange
        var startedHandlerMock = new Mock<IEventHandler<GenerationStartedEvent>>();
        startedHandlerMock.Setup(h => h.HandleAsync(It.IsAny<GenerationStartedEvent>()))
                        .Returns(Task.CompletedTask);

        var completedHandlerMock = new Mock<IEventHandler<GenerationCompletedEvent>>();
        completedHandlerMock.Setup(h => h.HandleAsync(It.IsAny<GenerationCompletedEvent>()))
                           .Returns(Task.CompletedTask);

        var services = new ServiceCollection();
        services.AddSingleton(startedHandlerMock.Object);
        services.AddSingleton(completedHandlerMock.Object);
        services.AddSingleton(_mockLogger.Object);
        var serviceProvider = services.BuildServiceProvider();

        var publisher = new EventAggregator(serviceProvider, _mockLogger.Object);

        // Act - publish Started event
        var startedEvent = new GenerationStartedEvent
        {
            RequestId = "test-request-3",
            ProjectPath = "/test/project3.csproj",
            EntityCount = 50,
            GeneratorTypes = ["TestGen"]
        };
        await publisher.PublishAsync(startedEvent);

        // Assert - only Started handlers were called
        startedHandlerMock.Verify(h => h.HandleAsync(It.IsAny<GenerationStartedEvent>()), Times.Once);
        completedHandlerMock.Verify(h => h.HandleAsync(It.IsAny<GenerationCompletedEvent>()), Times.Never);

        // Act - publish Completed event
        var completedEvent = new GenerationCompletedEvent
        {
            RequestId = "test-request-4",
            IsSuccessful = true,
            FilesGenerated = 10,
            ExecutionTimeMs = 150,
            Errors = []
        };
        await publisher.PublishAsync(completedEvent);

        // Assert - only Completed handlers were called
        startedHandlerMock.Verify(h => h.HandleAsync(It.IsAny<GenerationStartedEvent>()), Times.Once);
        completedHandlerMock.Verify(h => h.HandleAsync(It.IsAny<GenerationCompletedEvent>()), Times.Once);
    }

    [Fact]
    public async Task PublishAsync_WithHandlerException_ExceptionIsLoggedAndNotThrown()
    {
        // Arrange
        var handlerMock = new Mock<IEventHandler<GenerationStartedEvent>>();
        handlerMock.Setup(h => h.HandleAsync(It.IsAny<GenerationStartedEvent>()))
                  .ThrowsAsync(new InvalidOperationException("Test exception"));

        var services = new ServiceCollection();
        services.AddSingleton(handlerMock.Object);
        services.AddSingleton(_mockLogger.Object);
        var serviceProvider = services.BuildServiceProvider();

        var publisher = new EventAggregator(serviceProvider, _mockLogger.Object);
        var testEvent = new GenerationStartedEvent
        {
            RequestId = "test-request-5",
            ProjectPath = "/test/project5.csproj",
            EntityCount = 25,
            GeneratorTypes = ["FailingGenerator"]
        };

        // Act - should not throw
        var act = () => publisher.PublishAsync(testEvent);
        await act.Should().NotThrowAsync();

        // Assert - exception was logged (check that error was logged)
        _mockLogger.Verify(l => l.Log(
            LogLevel.Error,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => true),
            It.IsAny<InvalidOperationException>(),
            It.IsAny<Func<It.IsAnyType, Exception, string>>()!),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task PublishAsync_WithMultipleHandlersAndOneFails_OtherHandlersStillExecute()
    {
        // Arrange
        var successfulHandlerMock = new Mock<IEventHandler<GenerationStartedEvent>>();
        successfulHandlerMock.Setup(h => h.HandleAsync(It.IsAny<GenerationStartedEvent>()))
                           .Returns(Task.CompletedTask);

        var failingHandlerMock = new Mock<IEventHandler<GenerationStartedEvent>>();
        failingHandlerMock.Setup(h => h.HandleAsync(It.IsAny<GenerationStartedEvent>()))
                         .ThrowsAsync(new InvalidOperationException("Handler failed"));

        var anotherSuccessfulHandlerMock = new Mock<IEventHandler<GenerationStartedEvent>>();
        anotherSuccessfulHandlerMock.Setup(h => h.HandleAsync(It.IsAny<GenerationStartedEvent>()))
                                  .Returns(Task.CompletedTask);

        var services = new ServiceCollection();
        services.AddSingleton(successfulHandlerMock.Object);
        services.AddSingleton(failingHandlerMock.Object);
        services.AddSingleton(anotherSuccessfulHandlerMock.Object);
        services.AddSingleton(_mockLogger.Object);
        var serviceProvider = services.BuildServiceProvider();

        var publisher = new EventAggregator(serviceProvider, _mockLogger.Object);
        var testEvent = new GenerationStartedEvent
        {
            RequestId = "test-request-6",
            ProjectPath = "/test/project6.csproj",
            EntityCount = 75,
            GeneratorTypes = ["MixedGenerator"]
        };

        // Act - should not throw
        var act = () => publisher.PublishAsync(testEvent);
        await act.Should().NotThrowAsync();

        // Assert - all handlers were attempted
        successfulHandlerMock.Verify(h => h.HandleAsync(It.IsAny<GenerationStartedEvent>()), Times.Once);
        failingHandlerMock.Verify(h => h.HandleAsync(It.IsAny<GenerationStartedEvent>()), Times.Once);
        anotherSuccessfulHandlerMock.Verify(h => h.HandleAsync(It.IsAny<GenerationStartedEvent>()), Times.Once);
    }

    [Fact]
    public async Task PublishAsync_WithNullEvent_ThrowsArgumentNullException()
    {
        // Arrange
        var publisher = new EventAggregator(_serviceProvider, _mockLogger.Object);

        // Act
        Func<Task> act = () => publisher.PublishAsync<GenerationStartedEvent>(null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task PublishAsync_WithNoHandlers_CompletesWithoutError()
    {
        // Arrange
        var publisher = new EventAggregator(_serviceProvider, _mockLogger.Object);
        var testEvent = new GenerationStartedEvent
        {
            RequestId = "test-request-7",
            ProjectPath = "/test/project7.csproj",
            EntityCount = 0,
            GeneratorTypes = []
        };

        // Act - should not throw
        var act = () => publisher.PublishAsync(testEvent);
        await act.Should().NotThrowAsync();

        // Assert - warning was logged (check that warning was logged)
        _mockLogger.Verify(l => l.Log(
            LogLevel.Warning,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => true),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception, string>>()!),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task PublishAsync_EventPropertiesArePreserved()
    {
        // Arrange
        var handlerMock = new Mock<IEventHandler<GenerationStartedEvent>>();
        handlerMock.Setup(h => h.HandleAsync(It.IsAny<GenerationStartedEvent>()))
                  .Returns(Task.CompletedTask);

        var services = new ServiceCollection();
        services.AddSingleton(handlerMock.Object);
        services.AddSingleton(_mockLogger.Object);
        var serviceProvider = services.BuildServiceProvider();

        var publisher = new EventAggregator(serviceProvider, _mockLogger.Object);
        var requestId = "test-request-8";
        var testEvent = new GenerationStartedEvent
        {
            RequestId = requestId,
            ProjectPath = "/test/project8.csproj",
            EntityCount = 999,
            GeneratorTypes = ["Test"]
        };

        // Act
        await publisher.PublishAsync(testEvent);

        // Assert - handler received the same event instance
        handlerMock.Verify(h => h.HandleAsync(It.Is<GenerationStartedEvent>(e =>
            e.EventId == testEvent.EventId &&
            e.OccurredAt == testEvent.OccurredAt &&
            e.RequestId == requestId &&
            e.ProjectPath == testEvent.ProjectPath &&
            e.EntityCount == testEvent.EntityCount &&
            e.GeneratorTypes.SequenceEqual(testEvent.GeneratorTypes)
        )), Times.Once);
    }

    [Fact]
    public async Task PublishAsync_WithDifferentEventTypes_EachGetsOwnHandlers()
    {
        // Arrange
        var startedHandlerMock = new Mock<IEventHandler<GenerationStartedEvent>>();
        startedHandlerMock.Setup(h => h.HandleAsync(It.IsAny<GenerationStartedEvent>()))
                        .Returns(Task.CompletedTask);

        var completedHandlerMock = new Mock<IEventHandler<GenerationCompletedEvent>>();
        completedHandlerMock.Setup(h => h.HandleAsync(It.IsAny<GenerationCompletedEvent>()))
                           .Returns(Task.CompletedTask);

        var services = new ServiceCollection();
        services.AddSingleton(startedHandlerMock.Object);
        services.AddSingleton(completedHandlerMock.Object);
        services.AddSingleton(_mockLogger.Object);
        var serviceProvider = services.BuildServiceProvider();

        var publisher = new EventAggregator(serviceProvider, _mockLogger.Object);

        // Act - publish Started
        var startedEvent = new GenerationStartedEvent
        {
            RequestId = "test-request-9",
            ProjectPath = "/test/project9.csproj",
            EntityCount = 10,
            GeneratorTypes = ["A"]
        };
        await publisher.PublishAsync(startedEvent);

        // Act - publish Completed
        var completedEvent = new GenerationCompletedEvent
        {
            RequestId = "test-request-10",
            IsSuccessful = false,
            FilesGenerated = 5,
            ExecutionTimeMs = 200,
            Errors = ["Error occurred"]
        };
        await publisher.PublishAsync(completedEvent);

        // Assert - each event type went to its own handlers
        startedHandlerMock.Verify(h => h.HandleAsync(It.IsAny<GenerationStartedEvent>()), Times.Once);
        completedHandlerMock.Verify(h => h.HandleAsync(It.IsAny<GenerationCompletedEvent>()), Times.Once);
    }

    [Fact]
    public async Task PublishAsync_HandlersExecuteSequentially()
    {
        // Arrange
        var callOrder = new List<int>();

        var handler1Mock = new Mock<IEventHandler<GenerationStartedEvent>>();
        handler1Mock.Setup(h => h.HandleAsync(It.IsAny<GenerationStartedEvent>()))
                  .Callback(() => callOrder.Add(1))
                  .Returns(Task.CompletedTask);

        var handler2Mock = new Mock<IEventHandler<GenerationStartedEvent>>();
        handler2Mock.Setup(h => h.HandleAsync(It.IsAny<GenerationStartedEvent>()))
                  .Callback(() => callOrder.Add(2))
                  .Returns(Task.CompletedTask);

        var handler3Mock = new Mock<IEventHandler<GenerationStartedEvent>>();
        handler3Mock.Setup(h => h.HandleAsync(It.IsAny<GenerationStartedEvent>()))
                  .Callback(() => callOrder.Add(3))
                  .Returns(Task.CompletedTask);

        var services = new ServiceCollection();
        services.AddSingleton(handler1Mock.Object);
        services.AddSingleton(handler2Mock.Object);
        services.AddSingleton(handler3Mock.Object);
        services.AddSingleton(_mockLogger.Object);
        var serviceProvider = services.BuildServiceProvider();

        var publisher = new EventAggregator(serviceProvider, _mockLogger.Object);
        var testEvent = new GenerationStartedEvent
        {
            RequestId = "test-request-11",
            ProjectPath = "/test/project11.csproj",
            EntityCount = 1,
            GeneratorTypes = []
        };

        // Act
        await publisher.PublishAsync(testEvent);

        // Assert - handlers executed in order
        callOrder.Should().Equal([1, 2, 3]);
    }
}
