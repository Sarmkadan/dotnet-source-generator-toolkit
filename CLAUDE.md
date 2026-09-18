# CLAUDE.md

Roslyn-based .NET 10 CLI toolkit that generates repositories, mappers, validators and serializers from attributed entity classes, plus a real incremental generator (`AutoImplementGenerator`).

## Build

- SDK: .NET 10.0.100 (`global.json`, rollForward latestMinor). Solution file: `dotnet-source-generator-toolkit.slnx`.
- `dotnet restore && dotnet build --configuration Release --no-restore` (or `make build`)
- `dotnet run -- --path . --format Json` (`make run`, `make run-dry`, `make run-verbose`)
- `make lint` = build with `/p:EnforceCodeStyleInBuild=true`; `make pack`, `make publish-*`, `make docker-build`
- CI (`.github/workflows/`) builds and tests on ubuntu with .NET 8.0.x and 10.0.x matrix, Release config.

## Test

- `dotnet test --configuration Release --no-build --verbosity normal` (or `make test`; `make coverage` for opencover)
- Single test: `dotnet test tests/dotnet-source-generator-toolkit.Tests --filter "FullyQualifiedName~AutoImplementGenerator"`
- Stack: xUnit 2.9, FluentAssertions 8, Moq, `Microsoft.CodeAnalysis.CSharp` 4.8 for in-memory compilations.
- Snapshot tests: `tests/.../Generators/Snapshots/*.verified.txt` are embedded resources; `GeneratorTestHarness.cs` and `SnapshotFixtures.cs` drive generator runs.
- Benchmarks: `benchmarks/` (BenchmarkDotNet, separate csproj, not in the solution). `make benchmark` just times a CLI run.

## Format

- `dotnet format` (`make format`). Rules live in `.editorconfig`: 4-space indent, Allman braces, `System.*` usings first, private fields `_camelCase`, file-scoped namespaces preferred.

## Layout

Main project is the repo root (`dotnet-source-generator-toolkit.csproj`, root namespace `DotNetSourceGeneratorToolkit`); `tests/`, `benchmarks/`, `examples/` are excluded from its compile via `<Compile Remove>`.

- `Program.cs` - entry point; DI container setup (`ConfigureServices`), CLI dispatch (help/version/stats/generate).
- `CLI/` - `CliArgumentParser`, `CliOptions`.
- `Domain/` - `Entity`, `EntityProperty`, `GenerationOptions`, `GenerationResult`, `GenerationTemplate`, `ProjectInfo`, `SourceFile`, `ValidationResult`.
- `Services/` - generator services. `GeneratorServiceBase` (template-method base) -> `RepositoryGeneratorService`, `MapperGeneratorService`, `SerializerGeneratorService`, `ValidatorGeneratorService`. Also `TemplateEngineService`, `CodeEmitter`, `GeneratorDiagnostics` (diagnostic ID ranges), `GeneratorHintNameFormatter`, `IncrementalGeneratorService`.
- `Generators/AutoImplementGenerator.cs` - `IIncrementalGenerator` for `[GenerateToString]` / `[GenerateEquals]`; diagnostics `SGTK00x`.
- `Pipeline/` - `GenerationPipeline`, `IncrementalGenerationContext`.
- `Infrastructure/` - `EntityAnalyzer`, `AttributeAnalyzer`, `FileSystemService`, `ConfigurationManager`, `RetryPolicy`.
- `Middleware/` - `MiddlewarePipeline` with Logging/Validation/ErrorHandling middleware.
- `Events/` - `EventAggregator`, generation started/completed events, `GenerationMetricsCollector`.
- `Caching/`, `Batch/`, `Formatters/` (Json/Xml/Csv/Text output), `Integration/` (HttpClient, Webhook), `Repositories/`, `Metrics/`, `Configuration/`, `Constants/`, `Exceptions/`, `Utilities/`, `Extensions/`.
- `docs/` - per-type markdown reference; `README.md` is the full reference, `DEVELOPMENT.md` for dev setup, `GETTING_STARTED.md` for a quick path.
- `examples/v2-basic-usage/` - standalone example project included in the solution.

## Conventions

- Every `.cs` file starts with `#nullable enable` and the author header block (`Author: Vladyslav Zaiets | https://sarmkadan.com`). Keep it on new files.
- XML doc comments on all public APIs (`GenerateDocumentationFile` is on).
- Interface-per-service: `IFoo.cs` next to `Foo.cs`; register in `Program.ConfigureServices`.
- Companion partial files per type: `FooExtensions.cs`, `FooValidation.cs`, `FooJsonExtensions.cs`.
- Exceptions derive from `DotNetSourceGeneratorToolkitException`; use `ArgumentNullException.ThrowIfNull` guards.
- Logging via `Microsoft.Extensions.Logging`, `LoggerMessage` source-generated methods in pipeline code.
- Tests: `<Type>Tests.cs`, method names `Method_Scenario` or `Method_Scenario_Expected`, Arrange/Act/Assert comments; generator output asserted via snapshots, diagnostics by ID and severity.
- Commits: Conventional Commits (`feat(pipeline): ...`, `fix: ...`, `docs: ...`). No `Co-Authored-By` lines.
- Do not commit `.aider*`, stray `*.g.cs` at root, or `Services/*.backup` files; they are junk.
