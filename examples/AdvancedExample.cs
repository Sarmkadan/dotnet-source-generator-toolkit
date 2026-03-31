// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNetSourceGeneratorToolkit.Domain;

namespace DotNetSourceGeneratorToolkit.Examples;

/// Advanced example showing custom middleware, event handling, and batch processing
public class AdvancedExample
{
    [Repository]
    [Mapper]
    [Validator]
    public class BlogPost
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public int AuthorId { get; set; }
        public DateTime PublishedAt { get; set; } = DateTime.UtcNow;
        public List<string> Tags { get; set; } = [];
        public int ViewCount { get; set; } = 0;
        public bool IsPublished { get; set; } = false;
    }

    [Mapper]
    public class BlogPostDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public int AuthorId { get; set; }
        public List<string> Tags { get; set; } = [];
    }

    /// Custom middleware for performance tracking
    public class PerformanceTrackingMiddleware : IMiddleware
    {
        private readonly IMetricsCollector _metricsCollector;

        public PerformanceTrackingMiddleware(IMetricsCollector metricsCollector)
        {
            _metricsCollector = metricsCollector;
        }

        public async Task ExecuteAsync(
            GenerationContext context,
            Func<GenerationContext, Task> next)
        {
            var startTime = DateTime.UtcNow;
            var watch = System.Diagnostics.Stopwatch.StartNew();

            try
            {
                await next(context);
            }
            finally
            {
                watch.Stop();
                _metricsCollector.RecordTiming(
                    $"generation_{context.EntityName}",
                    TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds));
            }
        }
    }

    /// Event-driven architecture example
    public class BlogPostService
    {
        private readonly IBlogPostRepository _repository;
        private readonly IBlogPostMapper _mapper;
        private readonly IBlogPostValidator _validator;
        private readonly IEventAggregator _eventAggregator;
        private readonly IMetricsCollector _metricsCollector;

        public BlogPostService(
            IBlogPostRepository repository,
            IBlogPostMapper mapper,
            IBlogPostValidator validator,
            IEventAggregator eventAggregator,
            IMetricsCollector metricsCollector)
        {
            _repository = repository;
            _mapper = mapper;
            _validator = validator;
            _eventAggregator = eventAggregator;
            _metricsCollector = metricsCollector;

            // Subscribe to generation events
            _eventAggregator.Subscribe<GenerationStartedEvent>(OnGenerationStarted);
            _eventAggregator.Subscribe<GenerationCompletedEvent>(OnGenerationCompleted);
        }

        public async Task<BlogPostDto?> GetPublishedPostAsync(int postId)
        {
            _eventAggregator.Publish(new GenerationStartedEvent
            {
                EntityName = "BlogPost",
                ProjectPath = ".",
                StartTime = DateTime.UtcNow
            });

            var post = await _repository.FirstOrDefaultAsync(p =>
                p.Id == postId && p.IsPublished);

            if (post == null)
                return null;

            var dto = _mapper.MapToDto(post);

            _eventAggregator.Publish(new GenerationCompletedEvent
            {
                EntityName = "BlogPost",
                FilePath = $"/posts/{post.Id}",
                CompletionTime = DateTime.UtcNow,
                Duration = TimeSpan.Zero,
                FileSize = System.Text.Json.JsonSerializer.Serialize(dto).Length,
                Success = true
            });

            return dto;
        }

        public async Task<List<BlogPostDto>> GetPostsByTagAsync(string tag)
        {
            var posts = await _repository.WhereAsync(p =>
                p.IsPublished && p.Tags.Contains(tag));

            return posts
                .Select(_mapper.MapToDto)
                .ToList();
        }

        public async Task<BlogPostDto> PublishPostAsync(int postId)
        {
            var post = await _repository.GetByIdAsync(postId);
            if (post == null)
                throw new ArgumentException("Post not found");

            post.IsPublished = true;
            post.PublishedAt = DateTime.UtcNow;

            var validationResult = await _validator.ValidateAsync(post);
            if (!validationResult.IsValid)
                throw new InvalidOperationException("Post validation failed");

            await _repository.UpdateAsync(post);

            _metricsCollector.Record("posts_published", 1);

            return _mapper.MapToDto(post);
        }

        public async Task<List<BlogPostDto>> GetPopularPostsAsync(int count = 10)
        {
            var popularPosts = await _repository.GetPagedAsync(1, count);
            var sorted = popularPosts
                .OrderByDescending(p => p.ViewCount)
                .Take(count)
                .ToList();

            return sorted
                .Select(_mapper.MapToDto)
                .ToList();
        }

        private void OnGenerationStarted(GenerationStartedEvent @event)
        {
            Console.WriteLine($"[{@event.StartTime:HH:mm:ss}] Generation started: {@event.EntityName}");
        }

        private void OnGenerationCompleted(GenerationCompletedEvent @event)
        {
            Console.WriteLine(
                $"[{@event.CompletionTime:HH:mm:ss}] Generation completed: " +
                $"{@event.EntityName} in {@event.Duration.TotalMilliseconds}ms " +
                $"({@event.FileSize} bytes)");
        }
    }

    /// Example of batch processing multiple entities
    public class BatchProcessingExample
    {
        public async Task ProcessMultipleEntitiesAsync(
            ISourceGeneratorService service,
            List<string> projectPaths)
        {
            var allResults = new List<GenerationResult>();

            // Process projects in parallel batches
            var batchSize = 10;
            for (int i = 0; i < projectPaths.Count; i += batchSize)
            {
                var batch = projectPaths
                    .Skip(i)
                    .Take(batchSize)
                    .ToList();

                var batchTasks = batch.Select(path =>
                    service.AnalyzeProjectAsync(path)
                        .ContinueWith(async task =>
                        {
                            if (task.IsCompletedSuccessfully)
                            {
                                var projectInfo = task.Result;
                                return await service.GenerateAllAsync(projectInfo);
                            }
                            return new List<GenerationResult>();
                        })
                );

                var batchResults = await Task.WhenAll(batchTasks);
                allResults.AddRange(batchResults.SelectMany(r => r.Result));

                Console.WriteLine($"Processed batch {i / batchSize + 1} - " +
                    $"{allResults.Count} total results");
            }

            Console.WriteLine($"Batch processing complete: {allResults.Count} total entities processed");
        }
    }
}
