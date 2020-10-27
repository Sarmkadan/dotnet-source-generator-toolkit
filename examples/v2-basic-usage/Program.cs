#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNetSourceGeneratorToolkit.Domain;

// Example demonstrating v2.0 attribute-driven mapper generator with custom converters

// Define a simple entity with generation attributes
[Mapper]
public sealed class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public bool IsAvailable { get; set; } = true;
}

// Define a DTO for API responses
[Mapper]
public sealed class ProductDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string PriceDisplay { get; set; } = string.Empty; // Custom formatted price
    public string CreatedDateString { get; set; } = string.Empty; // Custom formatted date
    public string AvailabilityStatus { get; set; } = string.Empty; // Custom formatted boolean
}

// Custom converter for price formatting
public sealed class PriceConverter : IValueConverter<decimal, string>
{
    public string Convert(decimal value, object? context = null)
    {
        return $"${value:N2}";
    }
}

// Custom converter for date formatting
public sealed class DateConverter : IValueConverter<DateTime, string>
{
    public string Convert(DateTime value, object? context = null)
    {
        return value.ToString("yyyy-MM-dd");
    }
}

// Custom converter for boolean status
public sealed class AvailabilityConverter : IValueConverter<bool, string>
{
    public string Convert(bool value, object? context = null)
    {
        return value ? "In Stock" : "Out of Stock";
    }
}

// Usage example
public sealed class ProductService
{
    public ProductDto MapProductToDto(Product product)
    {
        // The mapper generator will automatically use custom converters
        // based on the IValueConverter<TSource, TDestination> interface
        var mapper = new ProductMapper();
        return mapper.MapToDto(product);
    }
}

Console.WriteLine("v2.0 Basic Usage Example - Attribute-driven mapper with custom converters");
Console.WriteLine("This example demonstrates the new v2.0 features:");
Console.WriteLine("- Attribute-driven mapper generation");
Console.WriteLine("- Custom value converters (IValueConverter<TSource, TDestination>)");
Console.WriteLine("- Automatic converter selection based on mapping context");
