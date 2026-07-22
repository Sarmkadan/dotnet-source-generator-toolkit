using DotNetSourceGeneratorToolkit.Batch;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace DotNetSourceGeneratorToolkit.Tests.Batch;

public sealed class BatchProcessorTests
{
    private readonly Mock<ILogger<BatchProcessor<int>>> _loggerMock;
    private readonly BatchProcessor<int> _batchProcessor;

    public BatchProcessorTests()
    {
        _loggerMock = new Mock<ILogger<BatchProcessor<int>>>();
        _batchProcessor = new BatchProcessor<int>(_loggerMock.Object);
    }

    [Fact]
    public async Task ProcessAsync_WithNullItems_ThrowsArgumentNullException()
    {
        // Arrange
        Func<int, Task> processor = _ => Task.CompletedTask;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _batchProcessor.ProcessAsync(null!, processor, 10));
    }

    [Fact]
    public async Task ProcessAsync_WithNullProcessor_ThrowsArgumentNullException()
    {
        // Arrange
        var items = new List<int> { 1, 2, 3 };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _batchProcessor.ProcessAsync(items, null!, 10));
    }

    [Fact]
    public async Task ProcessAsync_WithNonPositiveBatchSize_ThrowsArgumentException()
    {
        // Arrange
        var items = new List<int> { 1, 2, 3 };
        Func<int, Task> processor = _ => Task.CompletedTask;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _batchProcessor.ProcessAsync(items, processor, 0));
    }

    [Fact]
    public async Task ProcessAsync_WithEmptyInput_ReturnsEmptyResults()
    {
        // Arrange
        var items = Enumerable.Empty<int>();
        Func<int, Task> processor = _ => Task.CompletedTask;

        // Act
        var results = await _batchProcessor.ProcessAsync(items, processor, 10);

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public async Task ProcessAsync_WithSingleItem_ProcessesItemSuccessfully()
    {
        // Arrange
        var items = new List<int> { 42 };
        var processedItems = new List<int>();
        Func<int, Task> processor = item =>
        {
            processedItems.Add(item);
            return Task.CompletedTask;
        };

        // Act
        var results = await _batchProcessor.ProcessAsync(items, processor, 10);

        // Assert
        results.Should().HaveCount(1);
        results.First().IsSuccessful.Should().BeTrue();
        results.First().Item.Should().Be(42);
        processedItems.Should().ContainSingle().And.Contain(42);
    }

    [Fact]
    public async Task ProcessAsync_WithBatchSizeRespected_ProcessesInCorrectBatches()
    {
        // Arrange
        var items = Enumerable.Range(1, 25).ToList();
        var processedItems = new List<int>();
        Func<int, Task> processor = item =>
        {
            processedItems.Add(item);
            return Task.CompletedTask;
        };

        // Act
        var results = await _batchProcessor.ProcessAsync(items, processor, 10);

        // Assert
        results.Should().HaveCount(25);
        results.Should().AllSatisfy(r => r.IsSuccessful.Should().BeTrue());
        processedItems.Should().BeEquivalentTo(items);
        results.Should().AllSatisfy(r => r.ExecutionTimeMs.Should().BeGreaterThanOrEqualTo(0));
    }

    [Fact]
    public async Task ProcessAsync_WithPartialFinalBatch_ProcessesAllItems()
    {
        // Arrange
        var items = Enumerable.Range(1, 27).ToList();
        var processedItems = new List<int>();
        Func<int, Task> processor = item =>
        {
            processedItems.Add(item);
            return Task.CompletedTask;
        };

        // Act
        var results = await _batchProcessor.ProcessAsync(items, processor, 10);

        // Assert
        results.Should().HaveCount(27);
        results.Should().AllSatisfy(r => r.IsSuccessful.Should().BeTrue());
        processedItems.Should().BeEquivalentTo(items);
    }

    [Fact]
    public async Task ProcessAsync_WithErrorInOneItem_ContinuesProcessingAndReportsError()
    {
        // Arrange
        var items = new List<int> { 1, 2, 3, 4, 5 };
        var processedItems = new List<int>();
        var errorItem = 3;
        Func<int, Task> processor = async item =>
        {
            if (item == errorItem)
            {
                await Task.Delay(10); // Ensure some execution time
                throw new InvalidOperationException("Test error");
            }
            processedItems.Add(item);
            await Task.CompletedTask;
        };

        // Act
        var results = await _batchProcessor.ProcessAsync(items, processor, 2);

        // Assert
        results.Should().HaveCount(5);
        results.Count(r => r.IsSuccessful).Should().Be(4);
        results.Count(r => !r.IsSuccessful).Should().Be(1);

        var errorResult = results.First(r => !r.IsSuccessful);
        errorResult.Item.Should().Be(errorItem);
        errorResult.ErrorMessage.Should().Be("Test error");
        errorResult.IsSuccessful.Should().BeFalse();

        processedItems.Should().BeEquivalentTo(new[] { 1, 2, 4, 5 });
    }

    [Fact]
    public async Task ProcessAsync_WithMultipleErrors_ContinuesProcessingAllItems()
    {
        // Arrange
        var items = new List<int> { 1, 2, 3, 4, 5, 6 };
        var processedItems = new List<int>();
        var errorItems = new[] { 2, 5 };
        Func<int, Task> processor = async item =>
        {
            if (errorItems.Contains(item))
            {
                await Task.Delay(5);
                throw new InvalidOperationException($"Error for item {item}");
            }
            processedItems.Add(item);
            await Task.CompletedTask;
        };

        // Act
        var results = await _batchProcessor.ProcessAsync(items, processor, 3);

        // Assert
        results.Should().HaveCount(6);
        results.Count(r => r.IsSuccessful).Should().Be(4);
        results.Count(r => !r.IsSuccessful).Should().Be(2);

        var errorResults = results.Where(r => !r.IsSuccessful).ToList();
        errorResults.Should().HaveCount(2);
        errorResults[0].Item.Should().BeOneOf(errorItems);
        errorResults[1].Item.Should().BeOneOf(errorItems);
        errorResults.Should().AllSatisfy(r => r.IsSuccessful.Should().BeFalse());

        processedItems.Should().BeEquivalentTo(new[] { 1, 3, 4, 6 });
    }

    [Fact]
    public async Task ProcessAsync_WithProgressCallback_ReportsProgressCorrectly()
    {
        // Arrange
        var items = Enumerable.Range(1, 15).ToList();
        var progressReports = new List<BatchProgress>();
        var progress = new Progress<BatchProgress>(report => progressReports.Add(report));
        Func<int, Task> processor = _ => Task.CompletedTask;

        // Act
        var results = await _batchProcessor.ProcessAsync(items, processor, 5, progress);

        // Assert
        progressReports.Should().NotBeEmpty();
        progressReports.Should().HaveCountGreaterThanOrEqualTo(3); // At least one per batch

        var finalReport = progressReports.Last();
        finalReport.ProcessedCount.Should().Be(15);
        finalReport.TotalCount.Should().Be(15);
        finalReport.ErrorCount.Should().Be(0);
        finalReport.PercentComplete.Should().Be(100);
    }

    [Fact]
    public async Task ProcessAsync_WithExceptionInProcessor_StillReturnsResultsForAllItems()
    {
        // Arrange
        var items = new List<int> { 1, 2, 3, 4 };
        Func<int, Task> processor = async item =>
        {
            if (item == 3)
            {
                throw new Exception("Processing failed");
            }
            await Task.CompletedTask;
        };

        // Act
        var results = await _batchProcessor.ProcessAsync(items, processor, 2);

        // Assert
        results.Should().HaveCount(4);
        results.Count(r => r.IsSuccessful).Should().Be(3);
        results.Count(r => !r.IsSuccessful).Should().Be(1);
        results.First(r => r.Item == 3).IsSuccessful.Should().BeFalse();
    }

    [Fact]
    public async Task ProcessAsync_WithBatchSizeOne_ProcessesItemsSequentially()
    {
        // Arrange
        var items = new List<int> { 1, 2, 3 };
        var callOrder = new List<int>();
        Func<int, Task> processor = async item =>
        {
            callOrder.Add(item);
            await Task.Delay(10);
        };

        // Act
        var results = await _batchProcessor.ProcessAsync(items, processor, 1);

        // Assert
        results.Should().HaveCount(3);
        results.Should().AllSatisfy(r => r.IsSuccessful.Should().BeTrue());
        callOrder.Should().BeEquivalentTo(new[] { 1, 2, 3 });
    }

    [Fact]
    public async Task ProcessAsync_WithLargeBatchSize_ProcessesAllItemsInSingleBatch()
    {
        // Arrange
        var items = Enumerable.Range(1, 100).ToList();
        var processedItems = new List<int>();
        Func<int, Task> processor = item =>
        {
            processedItems.Add(item);
            return Task.CompletedTask;
        };

        // Act
        var results = await _batchProcessor.ProcessAsync(items, processor, 1000);

        // Assert
        results.Should().HaveCount(100);
        results.Should().AllSatisfy(r => r.IsSuccessful.Should().BeTrue());
        processedItems.Should().BeEquivalentTo(items);
    }

    [Fact]
    public async Task BatchResult_Properties_ShouldBeSetCorrectly()
    {
        // Arrange
        var item = new TestItem { Id = 1, Name = "Test" };
        var result = new BatchResult<TestItem>
        {
            Item = item,
            IsSuccessful = true,
            ErrorMessage = null,
            ExecutionTimeMs = 123
        };

        // Assert
        result.Item.Should().Be(item);
        result.IsSuccessful.Should().BeTrue();
        result.ErrorMessage.Should().BeNull();
        result.ExecutionTimeMs.Should().Be(123);
    }

    [Fact]
    public async Task BatchProgress_Properties_ShouldCalculateCorrectly()
    {
        // Arrange
        var progress = new BatchProgress
        {
            ProcessedCount = 75,
            TotalCount = 100,
            ErrorCount = 5
        };

        // Assert
        progress.ProcessedCount.Should().Be(75);
        progress.TotalCount.Should().Be(100);
        progress.ErrorCount.Should().Be(5);
        progress.PercentComplete.Should().Be(75);

        // Test edge case: zero total
        var zeroProgress = new BatchProgress
        {
            ProcessedCount = 0,
            TotalCount = 0,
            ErrorCount = 0
        };
        zeroProgress.PercentComplete.Should().Be(0);
    }

    private sealed class TestItem
    {
        public int Id { get; set; }
        public string? Name { get; set; }
    }
}
