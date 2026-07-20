#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNetSourceGeneratorToolkit.Events;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace DotNetSourceGeneratorToolkit.Tests;

/// <summary>
/// Unit tests for GenerationMetricsCollector to verify metrics collection and aggregation.
/// </summary>
public sealed class GenerationMetricsCollectorTests
{
    private readonly Mock<ILogger<GenerationMetricsCollector>> _mockLogger;
    private readonly GenerationMetricsCollector _collector;

    public GenerationMetricsCollectorTests()
    {
        _mockLogger = new Mock<ILogger<GenerationMetricsCollector>>();
        _collector = new GenerationMetricsCollector(_mockLogger.Object);
    }

    [Fact]
    public void GetSnapshot_ShouldReturnInitialMetrics_WhenNoEventsProcessed()
    {
        // Arrange
        var snapshot = _collector.GetSnapshot();

        // Assert
        Assert.Equal(0, snapshot.TotalGenerations);
        Assert.Equal(0, snapshot.SuccessfulGenerations);
        Assert.Equal(0, snapshot.FailedGenerations);
        Assert.Equal(0, snapshot.TotalDurationMs);
        Assert.Equal(0, snapshot.AverageDurationMs);
        Assert.Null(snapshot.FirstGenerationStart);
        Assert.NotNull(snapshot.LastGenerationEnd);
        Assert.Equal(0, snapshot.GenerationRatePerHour);
    }

    [Fact]
    public void HandleAsync_GenerationStartedEvent_ShouldIncrementTotalGenerations()
    {
        // Arrange
        var startedEvent = new GenerationStartedEvent
        {
            RequestId = "test-request-1",
            ProjectPath = "/test/project",
            EntityCount = 5,
            GeneratorTypes = new List<string> { "Repository", "Mapper" }
        };

        // Act
        _collector.HandleAsync(startedEvent).Wait();

        // Assert
        var snapshot = _collector.GetSnapshot();
        Assert.Equal(1, snapshot.TotalGenerations);
        Assert.Equal(0, snapshot.SuccessfulGenerations);
        Assert.Equal(0, snapshot.FailedGenerations);
    }

    [Fact]
    public void HandleAsync_GenerationStartedEvent_ShouldSetFirstGenerationStart()
    {
        // Arrange
        var startedEvent = new GenerationStartedEvent
        {
            RequestId = "test-request-2",
            ProjectPath = "/test/project",
            EntityCount = 3
        };

        // Act
        _collector.HandleAsync(startedEvent).Wait();

        // Assert
        var snapshot = _collector.GetSnapshot();
        Assert.NotNull(snapshot.FirstGenerationStart);
        Assert.InRange(snapshot.FirstGenerationStart.Value, DateTime.UtcNow.AddMinutes(-1), DateTime.UtcNow.AddMinutes(1));
    }

    [Fact]
    public void HandleAsync_GenerationCompletedEvent_Successful_ShouldUpdateMetrics()
    {
        // Arrange
        var startedEvent = new GenerationStartedEvent
        {
            RequestId = "test-request-3",
            ProjectPath = "/test/project",
            EntityCount = 2
        };
        _collector.HandleAsync(startedEvent).Wait();

        var completedEvent = new GenerationCompletedEvent
        {
            RequestId = "test-request-3",
            IsSuccessful = true,
            FilesGenerated = 10,
            ExecutionTimeMs = 1500,
            Errors = new List<string>()
        };

        // Act
        _collector.HandleAsync(completedEvent).Wait();

        // Assert
        var snapshot = _collector.GetSnapshot();
        Assert.Equal(1, snapshot.TotalGenerations);
        Assert.Equal(1, snapshot.SuccessfulGenerations);
        Assert.Equal(0, snapshot.FailedGenerations);
        Assert.Equal(1500, snapshot.TotalDurationMs);
        Assert.Equal(1500, snapshot.AverageDurationMs);
    }

    [Fact]
    public void HandleAsync_GenerationCompletedEvent_Failed_ShouldUpdateMetrics()
    {
        // Arrange
        var startedEvent = new GenerationStartedEvent
        {
            RequestId = "test-request-4",
            ProjectPath = "/test/project",
            EntityCount = 1
        };
        _collector.HandleAsync(startedEvent).Wait();

        var completedEvent = new GenerationCompletedEvent
        {
            RequestId = "test-request-4",
            IsSuccessful = false,
            FilesGenerated = 0,
            ExecutionTimeMs = 500,
            Errors = new List<string> { "Test error" }
        };

        // Act
        _collector.HandleAsync(completedEvent).Wait();

        // Assert
        var snapshot = _collector.GetSnapshot();
        Assert.Equal(1, snapshot.TotalGenerations);
        Assert.Equal(0, snapshot.SuccessfulGenerations);
        Assert.Equal(1, snapshot.FailedGenerations);
        Assert.Equal(500, snapshot.TotalDurationMs);
        Assert.Equal(500, snapshot.AverageDurationMs);
    }

    [Fact]
    public void HandleAsync_MultipleEvents_ShouldAccumulateMetricsCorrectly()
    {
        // Arrange
        var startedEvent1 = new GenerationStartedEvent
        {
            RequestId = "test-request-5a",
            ProjectPath = "/test/project1",
            EntityCount = 2
        };
        var completedEvent1 = new GenerationCompletedEvent
        {
            RequestId = "test-request-5a",
            IsSuccessful = true,
            FilesGenerated = 8,
            ExecutionTimeMs = 1000,
            Errors = new List<string>()
        };

        var startedEvent2 = new GenerationStartedEvent
        {
            RequestId = "test-request-5b",
            ProjectPath = "/test/project2",
            EntityCount = 3
        };
        var completedEvent2 = new GenerationCompletedEvent
        {
            RequestId = "test-request-5b",
            IsSuccessful = false,
            FilesGenerated = 0,
            ExecutionTimeMs = 300,
            Errors = new List<string> { "Error occurred" }
        };

        var startedEvent3 = new GenerationStartedEvent
        {
            RequestId = "test-request-5c",
            ProjectPath = "/test/project3",
            EntityCount = 1
        };
        var completedEvent3 = new GenerationCompletedEvent
        {
            RequestId = "test-request-5c",
            IsSuccessful = true,
            FilesGenerated = 5,
            ExecutionTimeMs = 1200,
            Errors = new List<string>()
        };

        // Act
        _collector.HandleAsync(startedEvent1).Wait();
        _collector.HandleAsync(completedEvent1).Wait();
        _collector.HandleAsync(startedEvent2).Wait();
        _collector.HandleAsync(completedEvent2).Wait();
        _collector.HandleAsync(startedEvent3).Wait();
        _collector.HandleAsync(completedEvent3).Wait();

        // Assert
        var snapshot = _collector.GetSnapshot();
        Assert.Equal(3, snapshot.TotalGenerations);
        Assert.Equal(2, snapshot.SuccessfulGenerations);
        Assert.Equal(1, snapshot.FailedGenerations);
        Assert.Equal(2500, snapshot.TotalDurationMs); // 1000 + 300 + 1200
        Assert.Equal(2500.0 / 3, snapshot.AverageDurationMs);
        Assert.Equal(66.66666666666666, snapshot.SuccessRate);
    }

    [Fact]
    public void SuccessRate_ShouldCalculateCorrectly()
    {
        // Arrange
        var startedEvent = new GenerationStartedEvent
        {
            RequestId = "test-request-6",
            ProjectPath = "/test/project",
            EntityCount = 1
        };
        _collector.HandleAsync(startedEvent).Wait();

        var completedEvent = new GenerationCompletedEvent
        {
            RequestId = "test-request-6",
            IsSuccessful = true,
            FilesGenerated = 5,
            ExecutionTimeMs = 800,
            Errors = new List<string>()
        };
        _collector.HandleAsync(completedEvent).Wait();

        // Act
        var snapshot = _collector.GetSnapshot();

        // Assert
        Assert.Equal(100.0, snapshot.SuccessRate);
    }

    [Fact]
    public void ToString_ShouldReturnFormattedMetrics()
    {
        // Arrange
        var startedEvent = new GenerationStartedEvent
        {
            RequestId = "test-request-7",
            ProjectPath = "/test/project",
            EntityCount = 2
        };
        _collector.HandleAsync(startedEvent).Wait();

        var completedEvent = new GenerationCompletedEvent
        {
            RequestId = "test-request-7",
            IsSuccessful = true,
            FilesGenerated = 10,
            ExecutionTimeMs = 2000,
            Errors = new List<string>()
        };
        _collector.HandleAsync(completedEvent).Wait();

        // Act
        var result = _collector.GetSnapshot().ToString();

        // Assert
        Assert.Contains("Generation Metrics:", result);
        Assert.Contains("Total: 1", result);
        Assert.Contains("Success: 1 (100.0%)", result);
        Assert.Contains("Failed: 0", result);
        Assert.Contains("Total Duration: 2000ms", result);
        Assert.Contains("Average Duration: 2000.00ms", result);
    }
}
