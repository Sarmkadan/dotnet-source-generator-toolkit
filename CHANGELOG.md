# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.0.0] - 2025-11-24

### Added
- **Stable release**: Production-ready with full test coverage
- **Incremental generation mode**: Only regenerate entities whose source has changed
- **XML documentation generation**: Automatically produce XML doc comments for all generated types
- **`--backup-existing` flag**: Safe file overwrite with automatic backup creation
- **`GenerationResultRepository`**: Persistence layer for tracking generation history
- **`ProjectMetadataService`**: Extracts assembly and project metadata for use in generated headers
- **Dependency injection wiring**: `ServiceCollectionExtensions.AddSourceGeneratorToolkit()` registers all services

### Changed
- Finalised public API surface — no further breaking changes planned
- `ToolkitOptions` default values reviewed and stabilised
- All generator services now return strongly-typed result objects instead of raw strings

### Fixed
- Fixed incorrect async method signatures in generated repository stubs
- Fixed namespace collision when entity name clashes with its own namespace segment
- Fixed binary serializer skipping nullable reference-type properties

## [0.9.0] - 2025-10-13

### Added
- **`MetricsCollector`**: Tracks generation time, cache hit rate, file sizes, and error counts per run
- **`IncrementalGeneratorService`**: Foundation for incremental builds; tracks entity hashes between runs
- **`IncrementalGenerationContext`**: Carries incremental state through the pipeline
- **`GenerationResultAggregatorService`**: Merges results across multiple generator passes into a single report

### Changed
- `SourceGeneratorService` now delegates to `IncrementalGeneratorService` when incremental mode is enabled
- Progress reporting switched to structured log events rather than console writes

### Fixed
- Fixed memory leak when processing projects with more than 100 entities in a single pass
- Fixed incorrect file path casing on case-sensitive Linux file systems

## [0.8.0] - 2025-09-01

### Added
- **Webhook support**: `WebhookService` and `IWebhookService` POST generation results to a configurable URL with retry logic
- **`HttpClientService`**: Shared HTTP client abstraction with configurable timeout and base address
- **`XmlOutputFormatter`**: Document-oriented XML output with proper encoding
- **`CsvOutputFormatter`**: Spreadsheet-compatible CSV with automatic header mapping
- **`TextOutputFormatter`**: Human-readable plain-text summary for terminal output
- **`FormatterFactory`**: Runtime formatter selection by name; supports registering custom formatters
- **`--format` CLI flag**: Choose output format at invocation time (`Json`, `Csv`, `Xml`, `Text`)

### Changed
- JSON formatter now includes run metadata (timestamp, entity count, toolkit version) in the output envelope

### Fixed
- Fixed XML formatter producing malformed output for entity names containing angle-bracket-like generic syntax

## [0.7.0] - 2025-08-11

### Added
- **Middleware pipeline**: `MiddlewarePipeline` and `IMiddlewarePipeline` implement chain-of-responsibility for the generation request
- **`LoggingMiddleware`**: Structured log entry/exit events with duration tracking
- **`ErrorHandlingMiddleware`**: Catches unhandled exceptions, records them to `GenerationException`, and continues the pipeline
- **`ValidationMiddleware`**: Pre-flight entity validation before generation begins
- **`IMiddleware` contract**: Implement to add custom pre/post processing steps

### Changed
- Generation pipeline refactored to pass context through the middleware chain before calling generator services
- Error reporting unified through `GenerationException` and `ValidationException`

## [0.6.0] - 2025-07-07

### Added
- **`MemoryCache`** and **`ICache`**: TTL-based in-memory cache for entity analysis results and generated code
- **`CacheKey`**: Composite key type that incorporates entity name, version hash, and format
- **`EventAggregator`**: Pub-sub event bus; components publish `GenerationStartedEvent` and `GenerationCompletedEvent`
- **`IEventPublisher` / `IEventSubscriber`**: Interfaces for decoupled component communication
- **`--clear-cache` CLI flag**: Invalidates all cached entries before the run begins
- **`enableCaching` / `cacheExpirationMinutes`** configuration options

### Fixed
- Fixed cache key collision when two entities shared the same short name in different namespaces

## [0.5.0] - 2025-06-03

### Added
- **Full CLI interface**: `CliArgumentParser` and `CliOptions` parse `--path`, `--output`, `--config`, `--verbose`, `--log-level`, `--dry-run`, `--help`, and `--version`
- **`BatchProcessor`** and **`IBatchProcessor`**: Parallel entity processing with configurable `maxDegreeOfParallelism` and `batchSize`
- **`--dry-run` flag**: Analyse and report without writing any files to disk
- **`--max-parallelism` and `--batch-size` flags**: Tune throughput at runtime
- **`ConfigurationLoader`**: Reads `toolkit-config.json` and merges with CLI overrides
- **`ConfigurationValidator`**: Validates configuration values on startup with descriptive error messages

### Changed
- `Program.cs` entry point refactored to use the new CLI parser; previous hard-coded paths removed

### Fixed
- Fixed argument parser treating quoted paths with spaces as two separate arguments

## [0.4.0] - 2025-05-12

### Added
- **`SerializerGeneratorService`** and **`ISerializerGeneratorService`**: Generates JSON, XML, and CSV serializers from `[Serializer]` attribute
- **Binary serializer path**: Compact binary output for performance-critical scenarios
- **`CodeFormatterService`**: Post-processes generated code through a formatter pass for consistent style
- **`FormattingService`**: Wraps formatter with configurable line-length and indentation settings
- **`TemplateEngineService`**: Lightweight string-interpolation engine used by all four generator services

### Changed
- All generator services now share `TemplateEngineService` instead of each containing inline string building

### Fixed
- Fixed incorrect using directives emitted when serializer target type was in a nested namespace

## [0.3.0] - 2025-04-21

### Added
- **`ValidatorGeneratorService`** and **`IValidatorGeneratorService`**: Generates fluent validation rules from `[Validator]` attribute
- Built-in rule library: `NotEmpty`, `Length`, `GreaterThan`, `PrecisionScale`, `Pattern`
- Async validation support: generated validators include `ValidateAsync()` overload
- **`ValidationResult`** domain type: carries field-level error messages back to callers
- Localized error message support via `MessageLanguage` attribute property

### Fixed
- Fixed property introspection skipping `init`-only setters in C# 9+ record types

## [0.2.0] - 2025-03-31

### Added
- **`MapperGeneratorService`** and **`IMapperGeneratorService`**: Generates bidirectional entity-to-DTO mappers from `[Mapper]` attribute
- Profile-based generation: `[Mapper(Profile = "ApiResponse")]` produces a dedicated mapper class per profile
- Nested object mapping and collection mapping with null-safety checks
- **`EntityProperty`** domain type: enriched property descriptor used across all generator services
- **`GenerationTemplate`** and **`SourceFile`** domain types: intermediate representations for generated artefacts

### Changed
- `EntityAnalyzer` now populates `EntityProperty` instead of raw `PropertyInfo` — richer metadata for downstream generators

### Fixed
- Fixed incorrect bidirectional mapping when source and destination property names differed only in casing

## [0.1.0] - 2025-03-14

### Added
- **Initial release**: Core project skeleton with Roslyn-based entity analysis
- **`AttributeAnalyzer`** and **`IAttributeAnalyzer`**: Scans syntax trees for generation attributes (`[Repository]`, `[Mapper]`, `[Validator]`, `[Serializer]`)
- **`EntityAnalyzer`** and **`IEntityAnalyzer`**: Extracts entity properties and metadata from Roslyn semantic model
- **`RepositoryGeneratorService`** and **`IRepositoryGeneratorService`**: Generates CRUD repository implementations (`GetAllAsync`, `GetByIdAsync`, `CreateAsync`, `UpdateAsync`, `DeleteAsync`, `GetPagedAsync`)
- **`Entity`**, **`ProjectInfo`**, **`GenerationResult`** domain types
- **`FileSystemService`** and **`IFileSystemService`**: Safe file write with directory creation
- **`GenerationPipeline`**: Orchestrates analysis → generation → output in sequence
- **`GenerationConstants`**: Shared string constants for attribute names and generated file suffixes
- **`ToolkitOptions`**: Configuration model with sensible defaults
- Basic structured logging throughout the pipeline
- MIT licence, `.editorconfig`, `.gitignore`, and GitHub Actions build workflow

---

## Planned

- gRPC service generation from entity definitions
- Entity Framework Core migration scaffolding
- REST API endpoint generation with OpenAPI annotations
- GraphQL schema generation
- Auto-generated unit test stubs
- Visual Studio extension for real-time generation
- Dependency injection configuration generator

## Support

- Bug reports: [GitHub Issues](https://github.com/Sarmkadan/dotnet-source-generator-toolkit/issues)
- Security issues: vladyslav@sarmkadan.com
- Questions: open a discussion on GitHub

---

Built by [Vladyslav Zaiets](https://sarmkadan.com)
