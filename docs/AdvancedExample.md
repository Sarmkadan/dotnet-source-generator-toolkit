# AdvancedExample

`AdvancedExample` is a data‑transfer and service‑oriented type used within the `dotnet-source-generator-toolkit` project to represent a blog‑post entity together with its associated tracking middleware and service dependencies. It aggregates the core properties of a blog post, provides flags for publication state, and exposes asynchronous operations for common post‑related workflows.

## API

### int Id
Unique identifier for the blog post.  
**Purpose:** Primary key used for storage and retrieval.  
**Throws:** None.

### string Title
Human‑readable title of the blog post.  
**Purpose:** Displayed in listings and detail views.  
**Throws:** None.

### string Content
Body content of the blog post, typically in markdown or HTML.  
**Purpose:** Stores the main article text.  
**Throws:** None.

### int AuthorId
Identifier of the user who authored the post.  
**Purpose:** Foreign key linking to the authors table.  
**Throws:** None.

### DateTime PublishedAt
Timestamp indicating when the post was published.  
**Purpose:** Used for sorting and determining visibility.  
**Throws:** None.

### List<string> Tags
Collection of tag strings associated with the post.  
**Purpose:** Enables categorisation and filtering by topic.  
**Throws:** None.

### int ViewCount
Number of times the post has been viewed.  
**Purpose:** Tracks popularity for analytics.  
**Throws:** None.

### bool IsPublished
Flag indicating whether the post is publicly visible.  
**Purpose:** Controls access in queries that filter unpublished content.  
**Throws:** None.

### PerformanceTrackingMiddleware
Instance of middleware that records performance metrics for operations performed on the post.  
**Purpose:** Allows profiling of async methods and database calls.  
**Throws:** None.

### BlogPostService
Reference to a service that encapsulates business logic for blog‑post manipulation.  
**Purpose:** Provides access to higher‑level operations such as publishing or retrieving posts.  
**Throws:** None.

### async Task ExecuteAsync()
Performs an unspecified asynchronous operation on the post (e.g., initialization or cleanup).  
**Parameters:** None.  
**Return:** A `Task` that completes when the operation finishes.  
**Throws:**  
- `InvalidOperationException` if required dependencies (`BlogPostService` or `PerformanceTrackingMiddleware`) are not set.  
- Any exception propagated from the underlying service or middleware.

### async Task<BlogPostDto?> GetPublishedPostAsync()
Attempts to retrieve a published blog post as a data‑transfer object.  
**Parameters:** None.  
**Return:** A `Task` yielding a `BlogPostDto` if a published post is found; otherwise `null`.  
**Throws:**  
- `InvalidOperationException` if the internal `BlogPostService` is unavailable.  
- Any exception thrown by the service layer (e.g., database access errors).

### async Task<List<BlogPostDto>> GetPostsByTagAsync()
Retrieves all blog posts associated with a specific tag.  
**Parameters:** None.  
**Return:** A `Task` yielding a list of `BlogPostDto` objects matching the tag criteria.  
**Throws:**  
- `InvalidOperationException` when `BlogPostService` is not initialized.  
- Service‑level exceptions such as `TimeoutException` or `SqlException`.

### async Task<BlogPostDto> PublishPostAsync()
Publishes the current post, setting `IsPublished` to true and persisting the change.  
**Parameters:** None.  
**Return:** A `Task` yielding the updated `BlogPostDto` representing the published post.  
**Throws:**  
- `InvalidOperationException` if the post is already published or required fields are missing.  
- Exceptions from the underlying service (e.g., validation failures, concurrency conflicts).

### async Task<List<BlogPostDto>> GetPopularPostsAsync()
Fetches a list of posts ordered by view count, representing the most popular content.  
**Parameters:** None.  
**Return:** A `Task` yielding a list of `BlogPostDto` objects sorted descending by `ViewCount`.  
**Throws:**  
- `InvalidOperationException` when dependencies are absent.  
- Any exception raised by the data‑access layer.

## Usage

```csharp
// Example 1: Creating a new post and publishing it via the service.
var example = new AdvancedExample
{
    Title = "Introduction to Source Generators",
    Content = "Source generators allow compile‑time code generation...",
    AuthorId = 42,
    Tags = new List<string> { "C#", "Roslyn", "Source Generators" }
};

await example.ExecuteAsync();               // Initialise middleware/services.
var publishedDto = await example.PublishPostAsync();
// publishedDto.IsPublished == true
```

```csharp
// Example 2: Retrieving popular posts and filtering by tag.
var example = new AdvancedExample
{
    // Dependencies would be injected via constructor or property assignment in real code.
    BlogPostService = new BlogPostService(),
    PerformanceTrackingMiddleware = new PerformanceTrackingMiddleware()
};

var popular = await example.GetPopularPostsAsync();
var csharpPosts = popular.Where(p => p.Tags.Contains("C#")).ToList();
```

## Notes
- The type does not provide thread‑safe guarantees; concurrent modification of fields such as `ViewCount` or `IsPublished` from multiple threads may lead to race conditions. External synchronisation is required when shared across threads.
- All asynchronous methods assume that the `BlogPostService` and `PerformanceTrackingMiddleware` properties have been assigned prior to invocation; otherwise they throw `InvalidOperationException`.
- The `List<string> Tags` field is mutable; callers should avoid exposing the internal list directly if immutability is desired.
- `PublishedAt` is set only by the consumer; the type does not automatically populate it on publish. Setting it to `DateTime.MinValue` may be interpreted as an unpublished state by downstream logic.
