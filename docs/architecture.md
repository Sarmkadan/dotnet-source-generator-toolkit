// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

# Architecture

This document describes the solution as it actually is in the code, not as an
aspiration. Everything referenced below exists in the repository; file paths are
given so claims can be checked.

## System Overview

The toolkit is a single console project (`dotnet-source-generator-toolkit.csproj`,
`net10.0`, `OutputType=Exe`) that contains **two distinct code-generation paths**:

1. **Runtime generation services** (`Services/`) - a CLI-driven tool that scans a
   project directory, extracts entity metadata, and produces repository / mapper /
   validator / serializer source as strings. This is template-based generation
   executed at *tool run time*, orchestrated from `Program.cs`.
2. **A real Roslyn incremental generator** (`Generators/AutoImplementGenerator.cs`) -
   an `IIncrementalGenerator` that plugs into the compiler and emits `ToString()`
   and value-equality members for partial classes annotated with
   `[GenerateToString]` / `[GenerateEquals]`. It ships its own marker attributes
   via `RegisterPostInitializationOutput` and reports diagnostics `SGTK001`-`SGTK003`.

Understanding this split is the single most important thing about the codebase:
the `Services/` layer does *not* run inside the compiler; only
`AutoImplementGenerator` does. The rest of the assembly is a conventional
layered console application around path 1.

`benchmarks/`, `tests/`, and `examples/` are excluded from the main compile via
`<Compile Remove>` in the csproj; the test project lives in
`tests/dotnet-source-generator-toolkit.Tests/`.

```
┌──────────────────────────────────────────────────────────────┐
│ Program.cs (composition root)  |  CLI/ (argument parsing)    │
└──────────────────────┬───────────────────────────────────────┘
                       │
┌──────────────────────▼───────────────────────────────────────┐
│ Services/  - SourceGeneratorService (analyze + orchestrate)  │
│            - Repository/Mapper/Validator/Serializer services │
│            - IncrementalGeneratorService (change detection)  │
│            - TemplateEngineService, ProjectMetadataService   │
└──────────────────────┬───────────────────────────────────────┘
                       │
┌──────────────────────▼───────────────────────────────────────┐
│ Infrastructure/ - AttributeAnalyzer, EntityAnalyzer,         │
│                   ConfigurationManager, FileSystemService    │
│ Domain/         - Entity, EntityProperty, ProjectInfo,       │
│                   GenerationResult, SourceFile, ...          │
└──────────────────────┬───────────────────────────────────────┘
                       │
┌──────────────────────▼───────────────────────────────────────┐
│ Cross-cutting: Events/ (EventAggregator), Caching/           │
│ (MemoryCache), Metrics/, Batch/, Middleware/, Integration/   │
│ (HttpClientService, WebhookService), Formatters/             │
└──────────────────────────────────────────────────────────────┘

Separate, compiler-hosted:
  Generators/AutoImplementGenerator.cs (IIncrementalGenerator)
```

## Components

### Composition root - `Program.cs`

`Program.Main` builds a `ServiceCollection`, resolves `ISourceGeneratorService`,
analyzes the project path given as `args[0]` (defaults to the current
directory), then runs the repository, mapper, and validator generators over the
discovered entities and logs the results. Failures log and exit with code 1.

Note what `Main` does *not* do today: it does not invoke the serializer
generator, the formatter factory, the middleware pipeline, or the webhook
service, even though all of them are registered in DI. Those components are
exercised through the library surface (`Extensions/ServiceCollectionExtensions.cs`)
and the examples, not by the default CLI flow. See "Known limitations".

### DI registration - `Extensions/ServiceCollectionExtensions.cs`

`AddSourceGeneratorToolkit(this IServiceCollection, ToolkitOptions?)` is the
public entry point for embedding the toolkit in a host application. Design
decisions here:

- **`TryAdd` semantics everywhere** - a host that already registered its own
  `ICache` or `IFileSystemService` keeps its registration. The toolkit never
  fights the host container.
- **Lifetimes**: infrastructure that is stateless or holds shared state
  (`MemoryCache`, `FileSystemService`, `ConfigurationManager`,
  `TemplateEngineService`, `ProjectMetadataService`) is singleton; analyzers,
  repositories, and generator services are scoped so a "scope = one generation
  run" convention is possible.
- `AddIncrementalGeneration()` is a smaller registration for hosts that only
  want change-detection-based selective regeneration without the full toolkit.

`Program.ConfigureServices` builds on `AddSourceGeneratorToolkit` and adds the
CLI-host-specific pieces: logging, `IEventPublisher`, the typed HTTP client
(`AddHttpClient<IHttpClientService, HttpClientService>`), `IFormatterFactory`,
`IWebhookService`, `IMiddlewarePipeline`, and the open-generic
`IBatchProcessor<>`.

### Runtime generation - `Services/`

- `SourceGeneratorService.AnalyzeProjectAsync(path)` walks `.cs` files via
  `IFileSystemService`, feeds file contents to `IEntityAnalyzer`, and returns a
  `ProjectInfo` with the discovered `Entity` list. `GenerateAllAsync` /
  `GenerateForEntityAsync` fan out to the per-kind generator services;
  `ValidateProjectAsync` produces a `ValidationResult`.
- `RepositoryGeneratorService`, `MapperGeneratorService`,
  `ValidatorGeneratorService`, `SerializerGeneratorService` - each turns an
  `Entity` (name, namespace, properties) into a `GenerationResult` containing
  generated source text. Generation is string/template based
  (`TemplateEngineService`), not Roslyn syntax-tree construction. That is a
  deliberate trade-off: templates are trivially readable and diffable in
  snapshot tests, at the cost of no syntactic guarantees on the output -
  `CodeFormatterService`/`FormattingService` exist to compensate on formatting.
- `IncrementalGeneratorService` builds an `IncrementalGenerationContext` by
  hashing/tracking source files between runs so unchanged entities can be
  skipped. It explicitly handles partial classes spanning multiple files: if
  any file of a partial class changed, the whole entity is considered dirty.
  See `docs/IncrementalGenerationContext.md` for the contract.

### Compiler-hosted generation - `Generators/`

`AutoImplementGenerator` is the reference implementation of a *correct* modern
incremental generator, and several decisions in it are worth recording:

- Uses `SyntaxProvider.ForAttributeWithMetadataName` rather than scanning all
  syntax nodes - this is the cheap, cache-friendly API and keeps the generator
  out of the IDE hot path.
- The pipeline model type (`Target`) is a `sealed record` holding
  `EquatableArray<string>` / `EquatableArray<DiagnosticInfo>` instead of
  `ImmutableArray`, because incremental caching is defeated by types without
  value equality. `DiagnosticInfo` carries id + location + type name instead of
  a `Diagnostic` instance for the same reason.
- Diagnostics are flowed *through* the pipeline and reported in
  `RegisterSourceOutput`, never from the transform, so they survive caching.
- `SGTK001` (not partial) and `SGTK003` (static type) are errors that suppress
  generation; `SGTK002` (no public instance properties) is a warning and
  generation still proceeds with a trivial body.

Snapshot, diagnostic, and caching behavior are covered by
`tests/.../Generators/AutoImplementGenerator*Tests.cs` using the harness in
`GeneratorTestHarness.cs`.

### Infrastructure - `Infrastructure/`

- `AttributeAnalyzer` / `EntityAnalyzer` - parse source (Roslyn
  `Microsoft.CodeAnalysis.CSharp` 4.8.0) to find generation attributes and
  extract entity metadata into `Domain` types.
- `ConfigurationManager` - loads file-based configuration; results are cached
  through `ICache`.
- `FileSystemService` - the only place that touches the disk. Everything else
  depends on `IFileSystemService`, which is what makes the services unit-testable
  without a real file system.

### Domain - `Domain/`

Plain models: `Entity`, `EntityProperty`, `ProjectInfo`, `GenerationResult`,
`GenerationTemplate`, `SourceFile`, `ValidationResult`, `SerializerFormat`.
Validation and mapping helpers live in sibling `*Validation.cs` /
`*Extensions.cs` files rather than on the models themselves, keeping the models
serialization-friendly.

### Cross-cutting

- **Events** (`Events/`): `EventAggregator` implements `IEventPublisher` with an
  async `PublishAsync<TEvent>` for `IDomainEvent` types
  (`GenerationStartedEvent`, `GenerationCompletedEvent`);
  `LoggingEventHandler` is the built-in subscriber.
- **Caching** (`Caching/`): `MemoryCache : ICache` - synchronous
  `TryGet/Set/Contains/Remove/Clear` with per-entry TTL (`CacheEntry`).
  In-process only, by design: the toolkit is a short-lived CLI, so a
  distributed cache would be complexity without payoff. `ICache` exists so a
  host can substitute one anyway.
- **Middleware** (`Middleware/`): `MiddlewarePipeline : IMiddlewarePipeline`
  composes `IMiddleware` steps (`LoggingMiddleware`, `ValidationMiddleware`,
  `ErrorHandlingMiddleware`) around a `MiddlewareContext`, chain-of-responsibility
  style, including a `DelegateMiddleware` adapter for lambda steps.
- **Batch** (`Batch/`): `BatchProcessor<T>` runs items with bounded parallelism
  and returns per-item `BatchResult<T>`. Registered as the open generic
  `typeof(IBatchProcessor<>)`.
- **Pipeline** (`Pipeline/`): `GenerationPipeline` is a stage-based runner
  returning a `PipelineResult`; `IncrementalGenerationContext` carries
  change-tracking state between runs.
- **Formatters** (`Formatters/`): `FormatterFactory : IFormatterFactory` maps a
  format string to `Json/Csv/Xml/TextOutputFormatter` (`IOutputFormatter`).
  Classic factory + strategy; adding a format means one class and one switch arm.
- **Integration** (`Integration/`): `HttpClientService` (typed `HttpClient` via
  `Microsoft.Extensions.Http`) and `WebhookService` for post-generation
  notifications.
- **Metrics** (`Metrics/`): `MetricsCollector : IMetricsCollector` for timing
  counters. Registered by hosts that want it; not wired into `Program.Main`.

## Data Flow (default CLI run)

```
args[0] (project path, defaults to cwd)
  → SourceGeneratorService.AnalyzeProjectAsync
      → IFileSystemService (enumerate + read .cs files)
      → IEntityAnalyzer.AnalyzeFileAsync (per file)
      → ProjectInfo { Entities }
  → per entity: RepositoryGeneratorService.GenerateRepositoryAsync
  → MapperGeneratorService.GenerateAllMappersAsync(entities)
  → ValidatorGeneratorService.GenerateAllValidatorsAsync(entities)
  → logging of GenerationResults
```

## Extension Points

Grounded in actual seams, in rough order of usefulness:

1. **Host integration**: call `AddSourceGeneratorToolkit()` from any app and
   resolve `ISourceGeneratorService`; override any dependency beforehand thanks
   to `TryAdd`.
2. **`IOutputFormatter` + `FormatterFactory`**: add an output format.
3. **`IMiddleware` / `IMiddlewarePipeline`**: wrap generation with custom
   pre/post steps (auditing, timing).
4. **`ICache`**: replace `MemoryCache` with a persistent/distributed cache.
5. **`IEventPublisher` subscribers**: react to
   `GenerationStartedEvent`/`GenerationCompletedEvent` (see
   `LoggingEventHandler` as the model).
6. **New generator service**: implement alongside
   `I{Repository,Mapper,Validator,Serializer}GeneratorService` and register it;
   `SourceGeneratorService` orchestration is the only place that needs to know.

## Known Limitations

Honest list, current as of this writing:

- `Program.Main` only exercises repository/mapper/validator generation; the
  serializer service, formatters, middleware pipeline, metrics, and webhook
  integration are registered but reachable only through the library API. The
  CLI options in `CLI/` are similarly richer than what `Main` consumes.
- The runtime generators emit source as strings; there is no compile check of
  the emitted code inside the tool itself (the snapshot tests are the safety
  net).
- `MemoryCache` is per-process; two concurrent tool runs share nothing.
- `EventAggregator` holds strong references to subscribers - fine for a
  short-lived CLI, a leak hazard if embedded in a long-lived host without
  unsubscribing.
- Entity discovery is file-based analysis of loose `.cs` files; it does not
  load a `Compilation` for the whole project, so cross-file semantic
  information (base types in other assemblies, etc.) is out of scope for the
  runtime path. The compiler-hosted `AutoImplementGenerator` does not have this
  limitation.
