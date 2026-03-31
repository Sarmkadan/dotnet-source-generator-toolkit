# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.2.0] - 2026-03-15

### Added
- **Batch processing improvements**: New `--batch-size` parameter for controlling entity processing batches
- **Webhook retry mechanism**: Configurable retry count and timeout for webhook delivery
- **Event filtering**: Subscribe to specific event types with pattern matching
- **CSV serialization**: Added CSV output formatter with header mapping support
- **Async validation support**: Validators now support async rules and custom validation logic
- **XML serialization with schema**: XML serializer generates validation schemas
- **Performance metrics**: Built-in metrics collector for monitoring generation performance
- **Cache statistics**: Detailed cache hit/miss reporting and metrics

### Changed
- **Improved error handling**: Better error messages with suggestions for common issues
- **Roslyn upgrade**: Updated to Microsoft.CodeAnalysis 4.8.0 for better syntax analysis
- **Configuration validation**: Stricter validation with detailed error reporting
- **Middleware pipeline**: More granular control over middleware execution order

### Fixed
- Fixed null reference exception in mapper generation for nullable types
- Fixed incorrect namespace resolution for nested classes
- Fixed cache key collision in high-volume scenarios
- Fixed webhook delivery failures on network timeouts
- Fixed incorrect line count reporting for large files

### Deprecated
- `ILegacyMapper` interface deprecated in favor of new `IMapperWithProfile`

## [1.1.0] - 2026-02-10

### Added
- **Repository pagination**: `GetPagedAsync()` method with configurable page size
- **Custom middleware support**: Extensible middleware pipeline for custom processing
- **Event aggregator**: Pub-sub system for decoupled component communication
- **Multiple output formats**: JSON, CSV, XML, and Text formatters
- **Configuration validation**: Automatic validation of configuration files on load
- **HTTP webhook integration**: Post generation results to external webhooks
- **In-memory caching**: Configuration and result caching with TTL
- **Batch processing**: Parallel entity processing with configurable degree of parallelism
- **Dry-run mode**: Preview generation without writing files (`--dry-run` flag)

### Changed
- **Breaking**: `ISourceGeneratorService.GenerateAsync()` renamed to `GenerateAllAsync()`
- **Architecture**: Refactored to middleware pipeline pattern for better extensibility
- **Entity analysis**: Now uses Roslyn semantic analyzer instead of reflection
- **Output handling**: Unified output formatting through factory pattern

### Fixed
- Fixed incorrect property mapping in generated mappers
- Fixed null reference exceptions in validators for optional properties
- Fixed file encoding issues with Unicode characters
- Fixed recursive mapping causing stack overflow

## [1.0.0] - 2026-01-15

### Added
- **Core code generation**: Repository, mapper, validator, and serializer generation
- **Repository generation**: Full CRUD operations with query methods
- **Mapper generation**: Bidirectional entity-to-DTO mapping
- **Validator generation**: Fluent validation rule creation
- **Serializer generation**: JSON and XML serialization support
- **CLI interface**: Full command-line argument parsing with help system
- **Attribute-based configuration**: Mark entities with `[Repository]`, `[Mapper]`, `[Validator]`, `[Serializer]`
- **File system operations**: Safe file writing with backup support
- **Logging support**: Structured logging throughout the pipeline
- **Configuration file support**: `toolkit-config.json` for centralized configuration
- **Error handling middleware**: Graceful error handling with recovery attempts
- **Type-safe DI**: Microsoft.Extensions.DependencyInjection integration

### Features
- Entity analysis using Roslyn syntax trees
- Property introspection and metadata extraction
- Automatic namespace resolution
- XML documentation comment generation
- Entity relationship detection
- Null-safety analysis
- Collection type handling

---

## Unreleased

### Planned for Future Releases

- **gRPC service generation**: Generate gRPC service definitions from entities
- **Database migration generation**: Create Entity Framework Core migrations from entities
- **API endpoint generation**: Generate REST API endpoints with OpenAPI documentation
- **GraphQL schema generation**: Generate GraphQL schemas from entities
- **Unit test generation**: Auto-generate unit tests with common scenarios
- **Performance benchmarking**: Built-in performance analysis tools
- **Language support**: C# 11 pattern matching enhancements, required properties
- **Custom code templates**: User-defined code generation templates
- **IDE integration**: Visual Studio extension for real-time code generation
- **Incremental generation**: Only regenerate changed entities for faster builds
- **Dependency injection generator**: Automatic DI container configuration generation

## How to Update

### From v1.0.0 to v1.1.0

1. Update NuGet package: `dotnet package update`
2. Update configuration to use new middleware pipeline
3. Replace `GenerateAsync()` calls with `GenerateAllAsync()`
4. Optionally configure webhooks in `toolkit-config.json`

### From v1.1.0 to v1.2.0

1. Update NuGet package: `dotnet package update`
2. No breaking changes - full backward compatibility
3. Optionally enable new batch processing features via configuration
4. Review new performance metrics available in MetricsCollector

## Upgrade Notes

### .NET Version Support
- **v1.0.0**: .NET 10.0+
- **v1.1.0**: .NET 10.0+ (recommended for production)
- **v1.2.0**: .NET 10.0+ (latest features and performance improvements)

### Stability
- **v1.0.0**: Stable for basic code generation
- **v1.1.0**: Production-ready with enterprise features
- **v1.2.0**: Recommended for all new projects

## Support & Security

- 🐛 **Bug Reports**: [GitHub Issues](https://github.com/Sarmkadan/dotnet-source-generator-toolkit/issues)
- 🔒 **Security Issues**: Email to vladyslav@sarmkadan.com
- 💬 **Questions**: Open a discussion on GitHub

---

Built with ❤️ by [Vladyslav Zaiets](https://sarmkadan.com)
