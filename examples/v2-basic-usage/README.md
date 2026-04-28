# v2.0 Basic Usage Example

This example demonstrates the new v2.0 features of the dotnet-source-generator-toolkit:

## Features Demonstrated

- **Attribute-driven mapper generation**: The `[Mapper]` attribute generates mapping code automatically
- **Custom value converters**: Implement `IValueConverter<TSource, TDestination>` for custom formatting
- **Automatic converter selection**: Converters are automatically applied based on the mapping context

## How to Use

### Prerequisites
- .NET 6.0 or later
- dotnet-source-generator-toolkit package

### Running the Example

```bash
cd examples/v2-basic-usage

dotnet run
```

### Expected Output
The example will output information about the v2.0 features and demonstrate how custom converters work.

## Key Concepts

1. **Entity Definition**: Use `[Mapper]` attribute on your entity classes
2. **Custom Converters**: Implement `IValueConverter<TSource, TDestination>` for custom formatting logic
3. **Automatic Generation**: The toolkit generates mapping code that automatically applies the appropriate converters

## See Also

- [Migration Guide to v2.0](../docs/migration-guide-v2.md)
- [Docker Guide](../docs/docker-guide.md)
