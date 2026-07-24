#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// Tests for GenerationResultAggregatorService to ensure proper aggregation
// of generation results under various edge cases.
// =============================================================================

using DotNetSourceGeneratorToolkit.Domain;
using DotNetSourceGeneratorToolkit.Infrastructure;
using DotNetSourceGeneratorToolkit.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace DotNetSourceGeneratorToolkit.Tests.Services;

/// <summary>
/// Tests for GenerationResultAggregatorService to verify correct behavior when
/// aggregating various combinations of generation results.
/// </summary>
public sealed class GenerationResultAggregatorServiceTests
{
    private readonly Mock<ILogger<GenerationResultAggregatorService>> _loggerMock;
    private readonly Mock<IFileSystemService> _fileSystemServiceMock;
    private readonly GenerationResultAggregatorService _service;

    public GenerationResultAggregatorServiceTests()
    {
        _loggerMock = new Mock<ILogger<GenerationResultAggregatorService>>();
        _fileSystemServiceMock = new Mock<IFileSystemService>();

        _service = new GenerationResultAggregatorService(
            _loggerMock.Object,
            _fileSystemServiceMock.Object);
    }

    #region Analyze Method Tests

    [Fact]
    public void Analyze_WithNullResults_ThrowsArgumentNullException()
    {
        // Act
        Action act = () => _service.Analyze(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>("because null input should be rejected");
    }

    [Fact]
    public void Analyze_WithEmptyResults_ReturnsEmptyReport()
    {
        // Arrange
        var results = Enumerable.Empty<GenerationResult>();

        // Act
        var report = _service.Analyze(results);

        // Assert
        report.Should().NotBeNull();
        report.TotalResults.Should().Be(0);
        report.SuccessCount.Should().Be(0);
        report.FailureCount.Should().Be(0);
        report.SkippedCount.Should().Be(0);
        report.TotalDurationMs.Should().Be(0);
        report.TotalLinesGenerated.Should().Be(0);
        report.TotalWarnings.Should().Be(0);
        report.TotalErrors.Should().Be(0);
        report.SuccessRate.Should().Be(0);
        report.AverageDuration.Should().Be(TimeSpan.Zero);
        report.ResultsByType.Should().BeEmpty();
        report.FailedResults.Should().BeEmpty();
        report.ReportGeneratedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Analyze_WithSingleSuccessfulResult_ReturnsCorrectReport()
    {
        // Arrange
        var result = new GenerationResult
        {
            EntityName = "User",
            GeneratorType = GeneratorType.Repository,
            Status = GenerationStatus.Completed,
            GeneratedCode = "public class UserRepository { }",
            OutputFilePath = "/src/UserRepository.cs",
            GenerationDurationMs = 150,
            CodeLineCount = 1
        };

        var results = new[] { result };

        // Act
        var report = _service.Analyze(results);

        // Assert
        report.Should().NotBeNull();
        report.TotalResults.Should().Be(1);
        report.SuccessCount.Should().Be(1);
        report.FailureCount.Should().Be(0);
        report.SkippedCount.Should().Be(0);
        report.TotalDurationMs.Should().Be(150);
        report.TotalLinesGenerated.Should().Be(1);
        report.TotalWarnings.Should().Be(0);
        report.TotalErrors.Should().Be(0);
        report.SuccessRate.Should().Be(100);
        report.AverageDuration.Should().Be(TimeSpan.FromMilliseconds(150));
        report.ResultsByType.Should().HaveCount(1).And.ContainKey(GeneratorType.Repository).WhoseValue.Should().Be(1);
        report.FailedResults.Should().BeEmpty();
    }

    [Fact]
    public void Analyze_WithMultipleResultsFromDifferentGeneratorTypes_RetainsAllResults()
    {
        // Arrange
        var results = new GenerationResult[]
        {
            new GenerationResult
            {
                EntityName = "User",
                GeneratorType = GeneratorType.Repository,
                Status = GenerationStatus.Completed,
                GeneratedCode = "public class UserRepository { }",
                OutputFilePath = "/src/UserRepository.cs",
                GenerationDurationMs = 100,
                CodeLineCount = 1
            },
            new GenerationResult
            {
                EntityName = "User",
                GeneratorType = GeneratorType.Mapper,
                Status = GenerationStatus.Completed,
                GeneratedCode = "public class UserMapper { }",
                OutputFilePath = "/src/UserMapper.cs",
                GenerationDurationMs = 120,
                CodeLineCount = 1
            },
            new GenerationResult
            {
                EntityName = "Product",
                GeneratorType = GeneratorType.Validator,
                Status = GenerationStatus.Completed,
                GeneratedCode = "public class ProductValidator { }",
                OutputFilePath = "/src/ProductValidator.cs",
                GenerationDurationMs = 80,
                CodeLineCount = 1
            }
        };

        // Act
        var report = _service.Analyze(results);

        // Assert
        report.Should().NotBeNull();
        report.TotalResults.Should().Be(3);
        report.SuccessCount.Should().Be(3);
        report.FailureCount.Should().Be(0);
        report.SkippedCount.Should().Be(0);
        report.TotalDurationMs.Should().Be(300);
        report.TotalLinesGenerated.Should().Be(3);
        report.TotalWarnings.Should().Be(0);
        report.TotalErrors.Should().Be(0);
        report.SuccessRate.Should().Be(100);
        report.AverageDuration.Should().Be(TimeSpan.FromMilliseconds(100));

        // Verify all generator types are tracked
        report.ResultsByType.Should().HaveCount(3);
        report.ResultsByType[GeneratorType.Repository].Should().Be(1);
        report.ResultsByType[GeneratorType.Mapper].Should().Be(1);
        report.ResultsByType[GeneratorType.Validator].Should().Be(1);

        // Verify no results are in failed list
        report.FailedResults.Should().BeEmpty();
    }

    [Fact]
    public void Analyze_WithFailedResultMixedWithSuccessfulResults_ReportsPartialFailure()
    {
        // Arrange
        var results = new GenerationResult[]
        {
            new GenerationResult
            {
                EntityName = "User",
                GeneratorType = GeneratorType.Repository,
                Status = GenerationStatus.Completed,
                GeneratedCode = "public class UserRepository { }",
                OutputFilePath = "/src/UserRepository.cs",
                GenerationDurationMs = 100,
                CodeLineCount = 1
            },
            new GenerationResult
            {
                EntityName = "Product",
                GeneratorType = GeneratorType.Mapper,
                Status = GenerationStatus.Failed,
                GeneratedCode = "",
                OutputFilePath = "/src/ProductMapper.cs",
                GenerationDurationMs = 50,
                CodeLineCount = 0
            },
            new GenerationResult
            {
                EntityName = "Order",
                GeneratorType = GeneratorType.Validator,
                Status = GenerationStatus.Completed,
                GeneratedCode = "public class OrderValidator { }",
                OutputFilePath = "/src/OrderValidator.cs",
                GenerationDurationMs = 75,
                CodeLineCount = 1
            }
        };

        // Act
        var report = _service.Analyze(results);

        // Assert
        report.Should().NotBeNull();
        report.TotalResults.Should().Be(3);
        report.SuccessCount.Should().Be(2);
        report.FailureCount.Should().Be(1);
        report.SkippedCount.Should().Be(0);
        report.TotalDurationMs.Should().Be(225);
        report.TotalLinesGenerated.Should().Be(2);
        report.TotalWarnings.Should().Be(0);
        report.TotalErrors.Should().Be(0); // Errors are counted separately
        report.SuccessRate.Should().BeApproximately(66.67, 0.01);
        report.AverageDuration.Should().Be(TimeSpan.FromMilliseconds(75));

        // Verify failed result is in failed list
        report.FailedResults.Should().HaveCount(1);
        report.FailedResults[0].EntityName.Should().Be("Product");
        report.FailedResults[0].GeneratorType.Should().Be(GeneratorType.Mapper);
        report.FailedResults[0].Status.Should().Be(GenerationStatus.Failed);
    }

    [Fact]
    public void Analyze_WithDuplicateIdenticalResults_TracksAllOccurrences()
    {
        // Arrange - same result submitted twice
        var baseResult = new GenerationResult
        {
            EntityName = "User",
            GeneratorType = GeneratorType.Repository,
            Status = GenerationStatus.Completed,
            GeneratedCode = "public class UserRepository { }",
            OutputFilePath = "/src/UserRepository.cs",
            GenerationDurationMs = 100,
            CodeLineCount = 1
        };

        var results = new[] { baseResult, baseResult };

        // Act
        var report = _service.Analyze(results);

        // Assert - duplicates are preserved (no deduplication logic in Analyze)
        report.Should().NotBeNull();
        report.TotalResults.Should().Be(2); // Both instances counted
        report.SuccessCount.Should().Be(2);
        report.FailureCount.Should().Be(0);
        report.TotalDurationMs.Should().Be(200);
        report.TotalLinesGenerated.Should().Be(2);
        report.SuccessRate.Should().Be(100);
        report.ResultsByType[GeneratorType.Repository].Should().Be(2);
    }

    [Fact]
    public void Analyze_WithResultsHavingWarningsAndErrors_CalculatesTotalsCorrectly()
    {
        // Arrange
        var resultWithWarnings = new GenerationResult
        {
            EntityName = "User",
            GeneratorType = GeneratorType.Repository,
            Status = GenerationStatus.Completed,
            GeneratedCode = "public class UserRepository { }",
            OutputFilePath = "/src/UserRepository.cs",
            GenerationDurationMs = 100,
            CodeLineCount = 1
        };
        resultWithWarnings.AddWarning("Potential performance issue detected");
        resultWithWarnings.AddWarning("Consider using async methods");

        var resultWithErrors = new GenerationResult
        {
            EntityName = "Product",
            GeneratorType = GeneratorType.Mapper,
            Status = GenerationStatus.Failed,
            GeneratedCode = "",
            OutputFilePath = "/src/ProductMapper.cs",
            GenerationDurationMs = 50,
            CodeLineCount = 0
        };
        resultWithErrors.AddError("Template compilation failed: Invalid syntax");
        resultWithErrors.AddError("Entity type not found");

        var results = new[] { resultWithWarnings, resultWithErrors };

        // Act
        var report = _service.Analyze(results);

        // Assert
        report.Should().NotBeNull();
        report.TotalResults.Should().Be(2);
        report.SuccessCount.Should().Be(1);
        report.FailureCount.Should().Be(1);
        report.TotalWarnings.Should().Be(2);
        report.TotalErrors.Should().Be(2); // 2 errors from failed result
    }

    [Fact]
    public void Analyze_WithMixedStatuses_CalculatesCountsCorrectly()
    {
        // Arrange
        var results = new GenerationResult[]
        {
            new GenerationResult
            {
                EntityName = "User",
                GeneratorType = GeneratorType.Repository,
                Status = GenerationStatus.Completed,
                GeneratedCode = "public class UserRepository { }",
                OutputFilePath = "/src/UserRepository.cs",
                GenerationDurationMs = 100,
                CodeLineCount = 1
            },
            new GenerationResult
            {
                EntityName = "Product",
                GeneratorType = GeneratorType.Mapper,
                Status = GenerationStatus.Failed,
                GeneratedCode = "",
                OutputFilePath = "/src/ProductMapper.cs",
                GenerationDurationMs = 50,
                CodeLineCount = 0
            },
            new GenerationResult
            {
                EntityName = "Order",
                GeneratorType = GeneratorType.Validator,
                Status = GenerationStatus.Skipped,
                GeneratedCode = "",
                OutputFilePath = "",
                GenerationDurationMs = 0,
                CodeLineCount = 0
            }
        };

        // Act
        var report = _service.Analyze(results);

        // Assert
        report.TotalResults.Should().Be(3);
        report.SuccessCount.Should().Be(1);
        report.FailureCount.Should().Be(1);
        report.SkippedCount.Should().Be(1);
    }

    #endregion

    #region GetStatistics Method Tests

    [Fact]
    public void GetStatistics_WithNullResults_ThrowsArgumentNullException()
    {
        // Act
        Action act = () => _service.GetStatistics(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>("because null input should be rejected");
    }

    [Fact]
    public void GetStatistics_WithEmptyResults_ReturnsEmptyStatistics()
    {
        // Arrange
        var results = Enumerable.Empty<GenerationResult>();

        // Act
        var stats = _service.GetStatistics(results);

        // Assert
        stats.Should().NotBeNull();
        stats.TotalCount.Should().Be(0);
        stats.CompletedCount.Should().Be(0);
        stats.FailedCount.Should().Be(0);
        stats.SuccessPercentage.Should().Be(0);
        stats.MinDurationMs.Should().Be(0);
        stats.MaxDurationMs.Should().Be(0);
        stats.AverageDurationMs.Should().Be(0);
        stats.TotalCodeLines.Should().Be(0);
        stats.TotalCodeBytes.Should().Be(0);
        stats.EntitiesProcessed.Should().Be(0);
        stats.ErrorCounts.Should().BeEmpty();
    }

    [Fact]
    public void GetStatistics_WithMultipleResults_CalculatesCorrectly()
    {
        // Arrange
        var results = new GenerationResult[]
        {
            new GenerationResult
            {
                EntityName = "User",
                GeneratorType = GeneratorType.Repository,
                Status = GenerationStatus.Completed,
                GeneratedCode = "public class UserRepository { }",
                OutputFilePath = "/src/UserRepository.cs",
                GenerationDurationMs = 100,
                CodeLineCount = 1
            },
            new GenerationResult
            {
                EntityName = "Product",
                GeneratorType = GeneratorType.Mapper,
                Status = GenerationStatus.Completed,
                GeneratedCode = "public class ProductMapper { }",
                OutputFilePath = "/src/ProductMapper.cs",
                GenerationDurationMs = 150,
                CodeLineCount = 1
            },
            new GenerationResult
            {
                EntityName = "Order",
                GeneratorType = GeneratorType.Validator,
                Status = GenerationStatus.Failed,
                GeneratedCode = "",
                OutputFilePath = "/src/OrderValidator.cs",
                GenerationDurationMs = 50,
                CodeLineCount = 0
            }
        };

        // Act
        var stats = _service.GetStatistics(results);

        // Assert
        stats.Should().NotBeNull();
        stats.TotalCount.Should().Be(3);
        stats.CompletedCount.Should().Be(2);
        stats.FailedCount.Should().Be(1);
        stats.SuccessPercentage.Should().BeApproximately(66.67, 0.01);
        stats.TotalCodeLines.Should().Be(2);
        stats.TotalCodeBytes.Should().BeGreaterThan(0);
        stats.EntitiesProcessed.Should().Be(3); // All have distinct entity names
        stats.MinDurationMs.Should().Be(100); // Only completed results are considered for duration stats
        stats.MaxDurationMs.Should().Be(150);
        stats.AverageDurationMs.Should().Be(125); // (100 + 150) / 2
        stats.ErrorCounts.Should().BeEmpty(); // No errors added to results
    }

    [Fact]
    public void GetStatistics_WithResultsHavingErrors_AggregatesErrorCounts()
    {
        // Arrange
        var result1 = new GenerationResult
        {
            EntityName = "User",
            GeneratorType = GeneratorType.Repository,
            Status = GenerationStatus.Failed
        };
        result1.AddError("Template: Syntax error");
        result1.AddError("Template: Missing required field");

        var result2 = new GenerationResult
        {
            EntityName = "Product",
            GeneratorType = GeneratorType.Mapper,
            Status = GenerationStatus.Failed
        };
        result2.AddError("Template: Syntax error"); // Same error type

        var results = new[] { result1, result2 };

        // Act
        var stats = _service.GetStatistics(results);

        // Assert
        stats.ErrorCounts.Should().HaveCount(1); // Both errors have same type "Template"
        stats.ErrorCounts["Template"].Should().Be(3); // 2 + 1
    }

    #endregion

    #region GenerateReport Method Tests

    [Fact]
    public void GenerateReport_WithNullReport_ThrowsArgumentNullException()
    {
        // Act
        Action act = () => _service.GenerateReport(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>("because null input should be rejected");
    }

    [Fact]
    public void GenerateReport_WithEmptyReport_ReturnsFormattedReport()
    {
        // Arrange
        var report = new GenerationReport
        {
            TotalResults = 0,
            SuccessCount = 0,
            FailureCount = 0,
            SkippedCount = 0,
            TotalDurationMs = 0,
            TotalLinesGenerated = 0,
            TotalWarnings = 0,
            TotalErrors = 0,
            SuccessRate = 0,
            AverageDuration = TimeSpan.Zero,
            ReportGeneratedAt = new DateTime(2024, 1, 1, 12, 0, 0)
        };

        // Act
        var reportText = _service.GenerateReport(report);

        // Assert
        reportText.Should().NotBeNullOrEmpty();
        reportText.Should().Contain("CODE GENERATION REPORT");
        reportText.Should().Contain("Total Results: 0");
        reportText.Should().Contain("Successful: 0");
        reportText.Should().Contain("Failed: 0");
        reportText.Should().Contain("Skipped: 0");
        reportText.Should().Contain("Success Rate: 0.00%");
        reportText.Should().Contain("Report Generated: 2024-01-01 12:00:00");
    }

    [Fact]
    public void GenerateReport_WithResults_ContainsAllSections()
    {
        // Arrange
        var results = new GenerationResult[]
        {
            new GenerationResult
            {
                EntityName = "User",
                GeneratorType = GeneratorType.Repository,
                Status = GenerationStatus.Completed,
                GeneratedCode = "public class UserRepository { }",
                OutputFilePath = "/src/UserRepository.cs",
                GenerationDurationMs = 100,
                CodeLineCount = 1
            }
        };

        var report = _service.Analyze(results);

        // Act
        var reportText = _service.GenerateReport(report);

        // Assert
        reportText.Should().Contain("SUMMARY");
        reportText.Should().Contain("PERFORMANCE");
        reportText.Should().Contain("BY GENERATOR TYPE");
        reportText.Should().Contain("ISSUES");
    }

    #endregion

    #region AnalyzeWithFileDiffAsync Method Tests

    [Fact]
    public async Task AnalyzeWithFileDiffAsync_WithNullResults_ThrowsArgumentNullException()
    {
        // Act
        Func<Task> act = async () => await _service.AnalyzeWithFileDiffAsync(null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>("because null input should be rejected");
    }

    [Fact]
    public async Task AnalyzeWithFileDiffAsync_WithEmptyResults_ReturnsReportWithEmptyDiff()
    {
        // Arrange
        var results = Enumerable.Empty<GenerationResult>();

        // Act
        var report = await _service.AnalyzeWithFileDiffAsync(results);

        // Assert
        report.Should().NotBeNull();
        report.TotalResults.Should().Be(0);
        report.FilesAdded.Should().Be(0);
        report.FilesChanged.Should().Be(0);
        report.FilesUnchanged.Should().Be(0);
        report.FilesCompared.Should().Be(0);
    }

    #endregion

    #region ExportToJsonAsync Method Tests

    [Fact]
    public async Task ExportToJsonAsync_WithNullReport_ThrowsArgumentNullException()
    {
        // Act
        Func<Task> act = async () => await _service.ExportToJsonAsync(null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>("because null input should be rejected");
    }

    [Fact]
    public async Task ExportToJsonAsync_WithValidReport_ReturnsJsonString()
    {
        // Arrange
        var results = new GenerationResult[]
        {
            new GenerationResult
            {
                EntityName = "User",
                GeneratorType = GeneratorType.Repository,
                Status = GenerationStatus.Completed,
                GeneratedCode = "public class UserRepository { }",
                OutputFilePath = "/src/UserRepository.cs",
                GenerationDurationMs = 100,
                CodeLineCount = 1
            }
        };

        var report = _service.Analyze(results);

        // Act
        var json = await _service.ExportToJsonAsync(report);

        // Assert
        json.Should().NotBeNullOrEmpty();
        json.Should().Contain("\"TotalResults\":");
        json.Should().Contain("\"SuccessCount\":");
        json.Should().Contain("\"ReportGeneratedAt\":");
    }

    #endregion

    #region GenerateFileDiffSummaryAsync Method Tests

    [Fact]
    public async Task GenerateFileDiffSummaryAsync_WithNullResults_ThrowsArgumentNullException()
    {
        // Act
        Func<Task> act = async () => await _service.GenerateFileDiffSummaryAsync(null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>("because null input should be rejected");
    }

    [Fact]
    public async Task GenerateFileDiffSummaryAsync_WithEmptyResults_ReturnsEmptySummary()
    {
        // Arrange
        var results = Enumerable.Empty<GenerationResult>();

        // Act
        var summary = await _service.GenerateFileDiffSummaryAsync(results);

        // Assert
        summary.Should().NotBeNull();
        summary.FilesAdded.Should().Be(0);
        summary.FilesChanged.Should().Be(0);
        summary.FilesUnchanged.Should().Be(0);
        summary.FilesCompared.Should().Be(0);
        summary.IsEmpty.Should().BeTrue();
        summary.HasChanges.Should().BeFalse();
    }

    [Fact]
    public async Task GenerateFileDiffSummaryAsync_WithCompletedResultsWithOutputPaths_GeneratesDiff()
    {
        // Arrange
        var results = new GenerationResult[]
        {
            new GenerationResult
            {
                EntityName = "User",
                GeneratorType = GeneratorType.Repository,
                Status = GenerationStatus.Completed,
                GeneratedCode = "public class UserRepository { }",
                OutputFilePath = "/src/UserRepository.cs"
            },
            new GenerationResult
            {
                EntityName = "Product",
                GeneratorType = GeneratorType.Mapper,
                Status = GenerationStatus.Completed,
                GeneratedCode = "public class ProductMapper { }",
                OutputFilePath = "/src/ProductMapper.cs"
            }
        };

        // Mock file system - assume files don't exist (would be added)
        _fileSystemServiceMock.Setup(fs => fs.FileExists(It.IsAny<string>()))
            .Returns(false);

        // Act
        var summary = await _service.GenerateFileDiffSummaryAsync(results);

        // Assert
        summary.Should().NotBeNull();
        summary.FilesCompared.Should().Be(2);
        summary.FilesAdded.Should().Be(2);
        summary.FilesChanged.Should().Be(0);
        summary.FilesUnchanged.Should().Be(0);
        summary.HasChanges.Should().BeTrue();
        summary.IsEmpty.Should().BeFalse();
    }

    #endregion
}
