#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

// Supporting abstractions for the v2.0 attribute-driven mapper example below.
// These mirror the toolkit's planned attribute-driven generation model:
// entities marked with [Mapper] get a matching *Mapper class, which can use
// custom IValueConverter<TSource, TDestination> implementations for
// non-trivial property conversions.

/// <summary>
/// Marks a class as a mapper generation target for the source generator toolkit.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class MapperAttribute : Attribute
{
}

/// <summary>
/// Converts a value of type <typeparamref name="TSource"/> to
/// <typeparamref name="TDestination"/>, optionally using additional context.
/// </summary>
public interface IValueConverter<TSource, TDestination>
{
    TDestination Convert(TSource value, object? context = null);
}

/// <summary>
/// Hand-written stand-in for the mapper the source generator toolkit would
/// emit for a <see cref="Product"/> class marked with <see cref="MapperAttribute"/>,
/// using the custom converters declared above for non-trivial conversions.
/// </summary>
public sealed class ProductMapper
{
    private readonly PriceConverter _priceConverter = new();
    private readonly DateConverter _dateConverter = new();
    private readonly AvailabilityConverter _availabilityConverter = new();

    public ProductDto MapToDto(Product product)
    {
        ArgumentNullException.ThrowIfNull(product);

        return new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            PriceDisplay = _priceConverter.Convert(product.Price),
            CreatedDateString = _dateConverter.Convert(product.CreatedDate),
            AvailabilityStatus = _availabilityConverter.Convert(product.IsAvailable),
        };
    }
}
