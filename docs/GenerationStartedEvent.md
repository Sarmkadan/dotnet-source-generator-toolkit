# GenerationStartedEvent

The `GenerationStartedEvent` class encapsulates information regarding the initiation of a source generation process within a project. It is intended for use in telemetry, diagnostic logging, or orchestration workflows where external systems need to react to or track the lifecycle of source code generation operations.

## API

*   `public string EventId`
    Represents the unique identifier for this specific instance of a generation event.
*   `public DateTime OccurredAt`
    Represents the UTC timestamp indicating when the generation process commenced.
*   `public string RequestId`
    Represents the identifier of the parent operation or request, enabling correlation with subsequent events in the same lifecycle.
*   `public string ProjectPath`
    Represents the absolute file system path of the project currently undergoing source generation.
*   `public int EntityCount`
    Represents the number of entities identified for processing during this specific generation cycle.
*   `public List<string> GeneratorTypes`
    Represents the collection of types or identifiers for the source generators scheduled to execute in this operation.

## Usage

### Example 1: Instantiating and Logging the Event

```csharp
var startedEvent = new GenerationStartedEvent
{
    EventId = Guid.NewGuid().ToString(),
    OccurredAt = DateTime.UtcNow,
    RequestId = "req-987654",
    ProjectPath = "/src/projects/MyLibrary.csproj",
    EntityCount = 42,
    GeneratorTypes = new List<string> { "Namespace.GeneratorA", "Namespace.GeneratorB" }
};

Console.WriteLine($"Generation started at {startedEvent.OccurredAt} for project: {startedEvent.ProjectPath}");
```

### Example 2: Handling the Event

```csharp
public void HandleGenerationStarted(GenerationStartedEvent e)
{
    if (e.GeneratorTypes.Count == 0)
    {
        Console.WriteLine("Warning: No generators scheduled.");
    }
    
    // Log telemetry
    TelemetryClient.TrackEvent("GenerationStarted", new Dictionary<string, string> {
        { "RequestId", e.RequestId },
        { "Project", e.ProjectPath }
    });
}
```

## Notes

*   **Data Integrity:** The `ProjectPath` property should contain an absolute path to ensure unambiguous identification of the project directory. The `GeneratorTypes` list should not contain null or empty entries.
*   **Thread Safety:** This class is a standard data container and is not thread-safe. If an instance is shared across threads, ensure that access is synchronized or treat the instance as immutable after initialization. If modifications are required in a multi-threaded context, use copies to prevent race conditions.
*   **Instantiation:** It is recommended to populate all properties immediately upon instantiation to maintain a valid state.
