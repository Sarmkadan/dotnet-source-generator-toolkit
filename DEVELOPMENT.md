// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

# Development Setup Guide

## Quick Setup

### Prerequisites
- .NET 10.0 SDK or later ([Download](https://dotnet.microsoft.com/en-us/download))
- Git
- Your favorite IDE (Visual Studio, VS Code, or Rider)

### 5-Minute Setup

```bash
# Clone repository
git clone https://github.com/Sarmkadan/dotnet-source-generator-toolkit.git
cd dotnet-source-generator-toolkit

# Restore dependencies
dotnet restore

# Build project
dotnet build

# Run tests
dotnet test

# Run the application
dotnet run -- --path . --format Json
```

## Development Workflow

### Building

```bash
# Debug build (faster, with symbols)
dotnet build

# Release build (optimized)
dotnet build -c Release

# Using Makefile
make build
make clean
make rebuild
```

### Testing

```bash
# Run all tests
dotnet test

# Run specific test file
dotnet test --filter ClassName

# Run with verbose output
dotnet test --verbosity detailed

# Generate coverage report
dotnet test /p:CollectCoverage=true /p:CoverageFormat=opencover
```

### Running Locally

```bash
# Basic run
dotnet run -- --path . --format Json

# With verbose output
dotnet run -- --path . --verbose

# Dry-run mode
dotnet run -- --path . --dry-run

# Using Makefile
make run
make run-verbose
make run-dry
```

### Code Formatting

```bash
# Format all code
dotnet format

# Check without modifying
dotnet format --verify-no-changes

# Using Makefile
make format
```

### Code Quality Checks

```bash
# Analyze code style
dotnet build /p:EnforceCodeStyleInBuild=true

# Using Makefile
make lint
```

## Project Structure

### Source Organization

```
dotnet-source-generator-toolkit/
├── CLI/                   # Command-line interface
├── Services/              # Core code generation services
├── Infrastructure/        # Low-level operations & Roslyn
├── Middleware/            # Pipeline middleware
├── Domain/                # Domain models
├── Repositories/          # Data persistence
├── Events/                # Event system
├── Caching/               # Caching implementation
├── Integration/           # HTTP & webhooks
├── Utilities/             # Helper extensions
├── Configuration/         # Config management
├── Metrics/               # Performance metrics
├── Batch/                 # Batch processing
├── Exceptions/            # Custom exceptions
├── Constants/             # Application constants
├── Formatters/            # Output formatters
├── Program.cs             # Entry point
└── dotnet-source-generator-toolkit.csproj
```

### Key Files to Understand

1. **Program.cs** - Dependency injection setup and entry point
2. **Services/SourceGeneratorService.cs** - Main orchestration service
3. **Infrastructure/AttributeAnalyzer.cs** - Roslyn-based attribute finding
4. **Middleware/MiddlewarePipeline.cs** - Pipeline execution
5. **Formatters/FormatterFactory.cs** - Output format selection

## Common Development Tasks

### Adding a New Generator

1. Create `Services/NewGeneratorService.cs` implementing `INewGenerator`
2. Register in `Program.cs` DI container
3. Add interface `Services/INewGenerator.cs`
4. Write tests in `Tests/Services/NewGeneratorServiceTests.cs`
5. Update documentation

Example:
```csharp
public interface INewGeneratorService
{
    Task<GenerationResult> GenerateAsync(Entity entity);
}

public class NewGeneratorService : INewGeneratorService
{
    public async Task<GenerationResult> GenerateAsync(Entity entity)
    {
        // Implementation
    }
}
```

### Adding a New Output Formatter

1. Create `Formatters/NewOutputFormatter.cs` implementing `IOutputFormatter`
2. Update `FormatterFactory.CreateFormatter()` to recognize new format
3. Add tests
4. Update documentation

Example:
```csharp
public class MarkdownOutputFormatter : IOutputFormatter
{
    public string Format(GenerationResult result) => /* markdown */;
    
    public Task<string> FormatAsync(GenerationResult result) => 
        Task.FromResult(Format(result));
}
```

### Adding Custom Middleware

1. Create `Middleware/CustomMiddleware.cs` implementing `IMiddleware`
2. Register in `MiddlewarePipeline` or DI container
3. Test the execution order and behavior

Example:
```csharp
public class CustomMiddleware : IMiddleware
{
    public async Task ExecuteAsync(
        GenerationContext context,
        Func<GenerationContext, Task> next)
    {
        // Pre-processing
        await next(context);
        // Post-processing
    }
}
```

## Git Workflow

### Branch Strategy

- **main** - Production-ready code
- **develop** - Integration branch for features
- **feature/feature-name** - Feature branches
- **fix/bug-name** - Bug fix branches

### Creating a Feature

```bash
# Create and checkout feature branch
git checkout -b feature/my-feature

# Make changes and commit
git add .
git commit -m "feat: add amazing feature"

# Push to remote
git push origin feature/my-feature

# Create pull request on GitHub
# After review and approval, merge to develop
```

### Commit Message Format

Follow conventional commits:
```
<type>(<scope>): <description>

<body>

<footer>
```

Types: feat, fix, docs, style, refactor, test, chore
Scope: the feature area (e.g., "mapper", "repository", "formatter")

Examples:
```
feat(mapper): add support for nested object mapping
fix(validator): resolve null reference exception in async validation
docs: update API reference with new methods
refactor(middleware): simplify error handling pipeline
test(serializer): add XML format tests
```

## Debugging

### In Visual Studio

1. Open solution file: `dotnet-source-generator-toolkit.sln`
2. Set breakpoints in code
3. Press F5 to start debugging
4. Use Debug output window for diagnostic messages

### In VS Code

1. Install C# extension
2. Create `.vscode/launch.json`:
```json
{
  "version": "0.2.0",
  "configurations": [
    {
      "name": ".NET Core Launch",
      "type": "csharp",
      "request": "launch",
      "program": "${workspaceFolder}/bin/Debug/net10.0/DotNetSourceGeneratorToolkit.dll",
      "args": ["--path", ".", "--verbose"],
      "cwd": "${workspaceFolder}",
      "stopAtEntry": false,
      "console": "internalConsole"
    }
  ]
}
```
3. Press F5 to debug

### Command-Line Debugging

```bash
# Run with verbose logging
dotnet run -- --path . --verbose --log-level Trace

# Debug output
dotnet run -- --path . --dry-run --verbose 2>&1 | head -50
```

## Performance Profiling

### Using dotnet-trace

```bash
# Collect trace
dotnet trace collect -- dotnet run -- --path . --verbose

# Analyze trace
perfview report trace.nettrace
```

### Using BenchmarkDotNet

Add to project:
```bash
dotnet add package BenchmarkDotNet
```

Create benchmark:
```csharp
[MemoryDiagnoser]
public class GenerationBenchmark
{
    [Benchmark]
    public async Task GenerateRepository() =>
        await service.GenerateRepositoryAsync(entity);
}
```

Run:
```bash
dotnet run -c Release -- --job short
```

## Testing Guidelines

### Unit Test Structure

```csharp
[TestClass]
public class RepositoryGeneratorServiceTests
{
    private RepositoryGeneratorService _service;
    private Mock<IFileSystemService> _fileSystemMock;
    
    [TestInitialize]
    public void Setup()
    {
        _fileSystemMock = new Mock<IFileSystemService>();
        _service = new RepositoryGeneratorService(_fileSystemMock.Object);
    }
    
    [TestMethod]
    public async Task GenerateAsync_WithValidEntity_CreatesFile()
    {
        // Arrange
        var entity = new Entity { Name = "User" };
        
        // Act
        var result = await _service.GenerateRepositoryAsync(entity);
        
        // Assert
        Assert.IsNotNull(result);
        _fileSystemMock.Verify(x => x.WriteAsync(It.IsAny<string>(), It.IsAny<string>()),
            Times.Once);
    }
}
```

### Test Best Practices

- Use AAA pattern (Arrange, Act, Assert)
- Test one thing per test
- Use descriptive test names
- Mock external dependencies
- Test both happy path and edge cases
- Aim for 80%+ code coverage

## Dependency Management

### Adding NuGet Packages

```bash
# Add package
dotnet add package Package.Name --version X.Y.Z

# Update packages
dotnet list package --outdated
dotnet upgrade package Package.Name

# Remove package
dotnet remove package Package.Name
```

### Updating Roslyn Dependencies

The project uses Microsoft.CodeAnalysis.CSharp for Roslyn APIs:

```bash
# Check for updates
dotnet package list

# Update to latest
dotnet add package Microsoft.CodeAnalysis.CSharp
```

## Documentation

### Adding Documentation

- Update README.md for major features
- Add .md files to docs/ for detailed guides
- Include code examples in documentation
- Update API reference for new public methods
- Update CHANGELOG.md with changes

### Building Documentation

HTML documentation can be generated from XML comments:

```bash
dotnet build --configuration Release /p:GenerateDocumentationFile=true
```

## Continuous Integration

The project uses GitHub Actions for CI:

- Builds on every push
- Runs tests on multiple platforms
- Publishes releases on tags
- Can be triggered manually

View workflows: `.github/workflows/`

## Release Process

### Creating a Release

1. Update version in `.csproj`
2. Update `CHANGELOG.md`
3. Commit and push to main
4. Create git tag: `git tag v1.x.x`
5. Push tag: `git push origin v1.x.x`
6. GitHub Actions automatically creates release and publishes NuGet package

## IDE Configuration

### Visual Studio

- Install C# Dev Kit
- Enable code analysis
- Configure StyleCop analyzer
- Set up auto-formatting on save

### VS Code

Extensions:
- C# (omnisharp)
- EditorConfig for VS Code
- C# Extensions

Settings:
```json
{
  "editor.formatOnSave": true,
  "omnisharp.enableEditorConfigSupport": true,
  "[csharp]": {
    "editor.defaultFormatter": "ms-dotnettools.csharp"
  }
}
```

### JetBrains Rider

- Import .editorconfig settings
- Enable Roslyn analyzers
- Configure code inspections
- Use built-in code formatter

## Troubleshooting Development Issues

### Build Failures

```bash
# Clean and rebuild
dotnet clean
dotnet restore
dotnet build

# Check for missing SDK
dotnet --version
dotnet --list-sdks
```

### Test Failures

```bash
# Run single test with output
dotnet test --filter ClassName.MethodName --verbosity detailed

# Check for test setup issues
dotnet test --no-restore --no-build
```

### Performance Issues

```bash
# Check build time
dotnet build -d:profile

# Analyze with dotnet-counters
dotnet counters monitor --process-id PID
```

## Resources

- [.NET Documentation](https://docs.microsoft.com/en-us/dotnet/)
- [Roslyn Documentation](https://docs.microsoft.com/en-us/dotnet/csharp/roslyn-sdk/)
- [C# Coding Guidelines](https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- [Project README](./README.md)
- [Architecture Guide](./docs/architecture.md)

---

**Questions?** Open an issue or discuss on GitHub.

Built with ❤️ by [Vladyslav Zaiets](https://sarmkadan.com)
