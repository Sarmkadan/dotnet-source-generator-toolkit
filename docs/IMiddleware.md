# IMiddleware

Represents the core middleware pipeline context for source generator execution. It carries request-scoped state—including CLI options, project metadata, generation results, and error collection—and exposes methods to add errors or short-circuit the pipeline. Implementations of `MiddlewareDelegate` process this context asynchronously.

## API

### MiddlewareDelegate

```csharp
public delegate Task MiddlewareDelegate(IMiddleware context);
```

A delegate that receives the current `IMiddleware` instance and returns a `Task`. Each middleware component in the pipeline conforms to this signature, enabling sequential or branching asynchronous processing of the generation context.

### RequestId

```csharp
public string RequestId { get; }
```

A unique identifier for the current generation request. Useful for correlating log output, diagnostics, and result sets across concurrent or sequential pipeline invocations.

### CliOptions

```csharp
public CLI.CliOptions? CliOptions { get; }
```

The parsed command-line options supplied to the tool, or `null` if no CLI configuration was provided. Middleware may inspect these options to alter behaviour (e.g., output paths, verbosity, target frameworks).

### ProjectInfo

```csharp
public Domain.ProjectInfo? ProjectInfo { get; }
```

Metadata about the project being processed, such as assembly name, target framework, and source file inventory. May be `null` when the pipeline runs outside a project context or before project resolution.

### GenerationResults

```csharp
public List<Domain.GenerationResult> GenerationResults { get; }
```

A mutable list of generation outputs produced by middleware components. Each entry typically contains generated source code, target file paths, and diagnostic information. Consumers append results as the pipeline progresses.

### Errors

```csharp
public List<string> Errors { get; }
```

A mutable list of human-readable error messages collected during pipeline execution. Middleware adds entries via `AddError`. The presence of any errors after pipeline completion conventionally indicates a failed generation run.

### StartTime

```csharp
public DateTime StartTime { get; }
```

The UTC timestamp captured when the pipeline began processing this request. Enables duration tracking and timeout enforcement by middleware.

### IsShortCircuited

```csharp
public bool IsShortCircuited { get; }
```

Indicates whether the pipeline has been short-circuited via `ShortCircuit()`. When `true`, downstream middleware should skip substantive work and pass the context through unchanged.

### AddError

```csharp
public void AddError(string error);
```

Appends an error message to the `Errors` list.

- **Parameters**: `error` — a non-null, non-empty string describing the failure.
- **Throws**: `ArgumentNullException` when `error` is `null`.

### ShortCircuit

```csharp
public void ShortCircuit();
```

Sets `IsShortCircuited` to `true`, signalling that the pipeline should terminate early. After calling this method, subsequent middleware should avoid performing generation work and simply forward the context.

## Usage

### Example 1: Logging middleware that records request duration

```csharp
public class TimingMiddleware
{
    public async Task InvokeAsync(IMiddleware context, MiddlewareDelegate next)
    {
        var sw = Stopwatch.StartNew();
        await next(context);
        sw.Stop();

        Console.WriteLine($"Request {context.RequestId} completed in {sw.ElapsedMilliseconds}ms");
    }
}
```

### Example 2: Validation middleware that short-circuits on missing project info

```csharp
public class ProjectValidationMiddleware
{
    public async Task InvokeAsync(IMiddleware context, MiddlewareDelegate next)
    {
        if (context.ProjectInfo is null)
        {
            context.AddError("ProjectInfo is required but was not resolved.");
            context.ShortCircuit();
            return;
        }

        await next(context);
    }
}
```

## Notes

- **Mutability**: `GenerationResults` and `Errors` are mutable lists shared across the pipeline. Middleware that enumerates them while other components may be writing concurrently must provide its own synchronisation.
- **Short-circuit semantics**: Calling `ShortCircuit()` does not automatically halt execution; it sets a flag that cooperative middleware must honour. A middleware that ignores `IsShortCircuited` will continue processing regardless.
- **Error accumulation**: `AddError` is safe to call from any middleware stage. There is no built-in deduplication—identical messages added multiple times will appear multiple times in `Errors`.
- **Thread safety**: The interface itself imposes no thread-safety guarantees. If the pipeline dispatches the same `IMiddleware` instance across multiple threads, external locking is required for `GenerationResults`, `Errors`, and `ShortCircuit`/`IsShortCircuited` interactions.
- **Lifetime**: `StartTime` is set once when the context is created and remains immutable thereafter. All other properties except `RequestId` and `StartTime` may change during pipeline execution.
