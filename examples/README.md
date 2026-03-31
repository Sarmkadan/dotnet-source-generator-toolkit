# .NET Source Generator Toolkit - Examples

This directory contains practical examples demonstrating various features and use cases of the toolkit.

## Examples

### 1. BasicExample.cs
**Demonstrates**: Repository, Mapper, and Validator generation

The simplest example showing:
- How to mark entities with generation attributes
- Generated repository CRUD operations
- Generated mappers for DTOs
- Generated validators

**Use case**: Simple CRUD operations with data access layer abstraction.

```bash
# Run basic example
dotnet run -- --path . --format Json
```

### 2. EcommerceExample.cs
**Demonstrates**: Multi-entity domain with relationships

A real-world e-commerce scenario including:
- Product management with repositories
- Order processing with complex domain
- Nested objects and collections
- Multiple entities working together
- Order validation with business rules

**Use case**: Enterprise application with multiple aggregate roots and repositories.

```bash
# Generate code for e-commerce domain
dotnet run -- --path ./examples --output ./Generated --format Json --verbose
```

### 3. AdvancedExample.cs
**Demonstrates**: Custom middleware, event handling, and batch processing

Advanced features including:
- Custom middleware for performance tracking
- Event-driven architecture with pub-sub
- Metrics collection and monitoring
- Batch processing multiple entities
- Event aggregator for decoupled communication

**Use case**: Large-scale applications requiring monitoring, metrics, and event-driven patterns.

```bash
# Run with verbose output to see events
dotnet run -- --path ./examples --verbose --format Json
```

## Running Examples

### Option 1: Direct Execution
```bash
dotnet build
dotnet run -- --path ./examples --format Json --output ./Generated
```

### Option 2: With Configuration File
Create `examples/toolkit-config.json`:
```json
{
  "enableCaching": true,
  "cacheExpirationMinutes": 60,
  "outputDirectory": "./Generated",
  "verboseLogging": true
}
```

Then run:
```bash
dotnet run -- --path ./examples --config ./examples/toolkit-config.json
```

### Option 3: Docker
```bash
docker build -t toolkit .
docker run --rm -v $(pwd)/examples:/workspace \
  toolkit \
  --path /workspace \
  --output /workspace/Generated
```

## Expected Output

After running the toolkit, you'll find generated code in the `Generated/` directory:

```
Generated/
├── Repositories/
│   ├── UserRepository.cs
│   ├── ProductRepository.cs
│   └── OrderRepository.cs
├── Mappers/
│   ├── UserMapper.cs
│   ├── ProductMapper.cs
│   └── OrderMapper.cs
├── Validators/
│   ├── UserValidator.cs
│   ├── ProductValidator.cs
│   └── OrderValidator.cs
└── Serializers/
    ├── ProductJsonSerializer.cs
    └── ProductXmlSerializer.cs
```

## Integration with Your Project

1. **Copy example entities** to your domain project
2. **Run the toolkit** on your project directory
3. **Register generated services** in your DI container
4. **Use generated code** in your application services

Example DI setup:
```csharp
services.AddScoped<IUserRepository, UserRepository>();
services.AddScoped<IUserMapper, UserMapper>();
services.AddScoped<IUserValidator, UserValidator>();
```

## Performance Characteristics

### BasicExample
- **Generation time**: ~200-300ms
- **Generated files**: 3 repositories, 1 mapper, 1 validator
- **Total lines of code**: ~500-700

### EcommerceExample
- **Generation time**: ~600-900ms
- **Generated files**: 5 repositories, 5 mappers, 3 validators, 2 serializers
- **Total lines of code**: ~2000-3000

### AdvancedExample
- **Generation time**: ~800-1200ms
- **Features**: Custom middleware, event handling, batch processing
- **Shows**: Enterprise-level patterns and monitoring

## Common Tasks

### Generate Repositories Only
Modify the example to include only `[Repository]` attributes.

### Skip Backup Files
```json
{
  "backupExistingFiles": false
}
```

### Custom Output Format
```bash
dotnet run -- --path ./examples --format Csv --output ./reports
```

### Dry Run (Preview)
```bash
dotnet run -- --path ./examples --dry-run --verbose
```

## Troubleshooting

### No entities generated?
- Ensure entities have `[Repository]`, `[Mapper]`, `[Validator]`, or `[Serializer]` attributes
- Check that .cs files are in the path you specified

### Permission denied error?
```bash
chmod -R 755 Generated/
```

### Out of memory?
Reduce parallelism:
```json
{
  "maxDegreeOfParallelism": 2
}
```

## Next Steps

1. Read the [Getting Started Guide](../docs/getting-started.md)
2. Check the [API Reference](../docs/api-reference.md)
3. Review the [Architecture Documentation](../docs/architecture.md)
4. Explore the [Deployment Guide](../docs/deployment.md)

## Contributing

Found a great example use case? Consider contributing:
1. Create a new example file
2. Include clear documentation
3. Submit a pull request

---

Built with ❤️ by [Vladyslav Zaiets](https://sarmkadan.com)
