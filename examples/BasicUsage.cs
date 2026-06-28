// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNetSourceGeneratorToolkit.Domain;

namespace DotNetSourceGeneratorToolkit.Examples;

/// <summary>
/// A minimal example showing the basic usage of the toolkit.
/// </summary>
public sealed class BasicUsage
{
    // Define an entity with generation attributes
    [Repository]
    [Validator]
    public sealed class Customer
    {
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }

    // After project build, the toolkit generates:
    // - ICustomerRepository
    // - CustomerRepository
    // - ICustomerValidator
    // - CustomerValidator
}
