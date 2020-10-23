// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

# Frequently Asked Questions

## General Questions

### Q: What is the .NET Source Generator Toolkit?

A: The toolkit is a Roslyn-powered code generation framework that automatically generates boilerplate code (repositories, mappers, validators, serializers) from C# attributes. It eliminates repetitive manual coding and keeps generated code synchronized with your domain models.

### Q: Who should use this toolkit?

A: Enterprise .NET developers, architects designing scalable applications, and teams looking to reduce boilerplate code and improve consistency across repositories, mappers, and validators.

### Q: Is this a template-based generator?

A: No. The toolkit uses Microsoft Roslyn's semantic analyzer for full syntax tree analysis and code generation. This enables intelligent generation with proper type safety, namespace resolution, and automatic updates when models change.

### Q: Can I use this in production?

A: Yes. The toolkit is production-ready with comprehensive error handling, logging, metrics collection, and webhook integration for monitoring.

### Q: What .NET versions are supported?

A: .NET 10.0 and later. The toolkit requires modern C# language features (12.0+) available in .NET 10.

## Technical Questions

### Q: How does code generation work?

A: 
1. The AttributeAnalyzer uses Roslyn to scan your project for generation attributes (`[Repository]`, `[Mapper]`, `[Validator]`, `[Serializer]`)
2. EntityAnalyzer extracts property information and type metadata
3. Service generators create syntactically correct C# code
4. Output formatters prepare the results in your chosen format
5. FileSystemService writes to disk (with optional backup)

### Q: Can I generate code for abstract classes or interfaces?

A: Currently, the toolkit generates code for concrete classes. Generated code will throw an exception if you attempt to analyze abstract classes or interfaces. Future versions may support interface-based generation.

### Q: Does the toolkit support nested objects and collections?

A: Yes. Mappers automatically handle:
- Nested object mapping (deep cloning with custom rules)
- Collection mapping (List<T>, IEnumerable<T>, arrays)
- Nullable reference types
- Complex property types

### Q: How are dependencies resolved in generated code?

A: Generated repositories expect a DbContext or UnitOfWork pattern. Inject your data access layer:

```csharp
services.AddScoped<IUserRepository>(sp => 
    new UserRepository(sp.GetRequiredService<AppDbContext>()));
```

### Q: Can I use custom attributes for generation?

A: You can create custom attributes derived from the toolkit's base attributes. The framework will recognize and process them. See [architecture.md](./architecture.md#extensibility-points) for custom middleware examples.

### Q: What about performance? Is generated code optimized?

A: Yes. Generated code includes:
- Efficient queries with eager loading support
- Async/await for non-blocking operations
- Caching where beneficial
- Batch operations for bulk processing
- Minimal reflection (compile-time generation)

## Configuration Questions

### Q: Where should I put toolkit-config.json?

A: Place it in your project root or specify a custom path via CLI:
```bash
dotnet run -- --config ./config/toolkit-config.json
```

### Q: Can I use environment variables in configuration?

A: The ConfigurationManager supports variable substitution:
```json
{
  "outputDirectory": "$GENERATED_PATH/code"
}
```

Environment variables are expanded at load time.

### Q: What's the impact of caching on generated code freshness?

A: Caching stores configuration and analysis results, not generated code. When you modify your entities, the toolkit invalidates affected caches. Explicit cache clearing:
```bash
dotnet run -- --clear-cache --path .
```

### Q: Can I disable XML documentation comments?

A: Yes, via configuration:
```json
{
  "generateXmlComments": false
}
```

## Usage Questions

### Q: How do I generate code for only specific entities?

A: Use selective attributes. Only classes with `[Repository]`, `[Mapper]`, etc. will have code generated:

```csharp
[Repository]  // Generated
public class User { }

public class Helper { }  // Not generated
```

### Q: Can I customize generated method names?

A: Not currently through attributes, but you can extend generated code with partial classes:

```csharp
// Generated
public partial class UserRepository { }

// Your custom additions
public partial class UserRepository
{
    public async Task<User?> FindByUsernameAsync(string username)
    {
        // Custom implementation
    }
}
```

### Q: How do I exclude properties from generation?

A: Use the `[IgnoreGeneration]` attribute:

```csharp
[Repository]
public class User
{
    public int Id { get; set; }
    
    [IgnoreGeneration]
    public string InternalField { get; set; } = string.Empty;
}
```

### Q: Can I generate code for DTOs with different property names?

A: Yes, use the `[MapProperty]` attribute:

```csharp
[Mapper]
public class User
{
    public int Id { get; set; }
    
    [MapProperty("FullName")]
    public string Name { get; set; } = string.Empty;
}

public class UserDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
}
```

### Q: How do I handle validation of related entities?

A: Use custom validation rules:

```csharp
[Validator]
public class Order
{
    public int CustomerId { get; set; }
    
    [ValidateAsync(typeof(CustomerValidationRule))]
    public Customer? Customer { get; set; }
}
```

## Integration Questions

### Q: How do I integrate with my existing data access layer?

A: The generated repositories are abstract. Implement the concrete DbContext:

```csharp
// Generated interface
public partial class UserRepository : IUserRepository
{
    protected DbContext Context { get; }
    
    public UserRepository(AppDbContext context) : base(context) { }
}
```

### Q: Can I publish generation results to a webhook?

A: Yes, configure webhooks in toolkit-config.json:

```json
{
  "webhookEnabled": true,
  "webhookUrl": "https://api.example.com/generation-webhook",
  "webhookRetries": 3
}
```

The toolkit will POST generation results after completion.

### Q: How do I integrate with CI/CD pipelines?

A: See [deployment.md](./deployment.md#cicd-integration) for examples with GitHub Actions, GitLab CI, and Azure Pipelines.

### Q: Can I generate code in a Docker container?

A: Yes, a Dockerfile is provided. See [deployment.md](./deployment.md#docker-deployment) for containerized deployment.

## Troubleshooting Questions

### Q: "No entities found for generation" - How do I fix this?

A: Ensure your classes have generation attributes:

```csharp
// Before (not generated)
public class User { }

// After (generated)
[Repository]
[Mapper]
public class User { }
```

### Q: Generated code won't compile - What went wrong?

A: Common causes:
1. Missing using statements - verify default namespace
2. Type mismatch in mappers - check property types match
3. Circular references - consider alternative mapping strategies

Run with verbose logging:
```bash
dotnet run -- --path . --verbose
```

### Q: How do I debug generated code issues?

A: Use dry-run mode to see what would be generated without writing:

```bash
dotnet run -- --path . --dry-run --verbose --format Json
```

Then examine the JSON output to identify issues.

### Q: Is the generated code safe for production?

A: Yes. Generated code follows Microsoft best practices:
- No dynamic code execution
- Full async/await support
- Null safety checks
- Proper exception handling
- Type-safe interfaces

### Q: Can I regenerate code without overwriting existing changes?

A: Enable backups:

```json
{
  "backupExistingFiles": true
}
```

Backups are saved with `.bak` extension before overwriting.

## Performance Questions

### Q: How long does generation typically take?

A: For 50-100 entities:
- Analysis: 1-2 seconds
- Generation: 2-5 seconds
- Formatting/Output: <1 second

For 1000+ entities, increase timeout and adjust parallelism.

### Q: What's the memory footprint?

A: Approximately 100-200MB for typical projects. Large projects may require higher limits. Monitor with:

```bash
# Monitor memory usage
dotnet run -- --path . 2>&1 | grep -i memory
```

### Q: Can I parallelize generation?

A: Yes, the toolkit uses configurable parallelism:

```json
{
  "maxDegreeOfParallelism": 4,
  "batchSize": 10
}
```

For CPU-bound operations, use 2-4 threads; adjust based on system resources.

### Q: How do I optimize for large projects?

A: 
1. Use batch processing with appropriate batch size
2. Reduce parallelism if memory is constrained
3. Enable caching to reuse analysis results
4. Use code formatting selectively
5. Consider splitting generation into phases

## Contributing Questions

### Q: How can I contribute?

A: Fork the repository and create a pull request with:
- Clear description of changes
- Unit tests for new features
- Updated documentation
- Adherence to C# conventions

See README.md for contribution guidelines.

### Q: Can I add a new output formatter?

A: Yes! Implement `IOutputFormatter` and register in `FormatterFactory`:

```csharp
public class MyCustomFormatter : IOutputFormatter
{
    public string Format(GenerationResult result) => /* implementation */;
}
```

Then update FormatterFactory:
```csharp
public IOutputFormatter CreateFormatter(string format) => format.ToLower() switch
{
    "custom" => new MyCustomFormatter(),
    // ...
};
```

### Q: Can I add support for additional attribute types?

A: Yes. Add new attributes to `Domain/` and extend service generators accordingly. See architecture documentation for extension points.

## Support

### Q: Where can I report bugs?

A: File issues on GitHub: [Issues](https://github.com/Sarmkadan/dotnet-source-generator-toolkit/issues)

### Q: Is there commercial support available?

A: The toolkit is MIT-licensed open source. For professional services, training, or custom development, contact: vladyslav@sarmkadan.com

### Q: How do I stay updated?

A: 
- Star the GitHub repository
- Watch release notifications
- Subscribe to CHANGELOG.md
- Follow updates at https://sarmkadan.com

### Q: Can I use this in my closed-source project?

A: Yes! The MIT license permits commercial use. See LICENSE file for details.
