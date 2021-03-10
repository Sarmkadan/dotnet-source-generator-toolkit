# AttributeAnalyzer

The `AttributeAnalyzer` type provides a simple way to inspect and extract information about attributes applied to a code symbol within a source‑generator context. It encapsulates the logic for enumerating attributes, checking for the presence of a specific attribute, and retrieving attribute constructor or named‑argument values as key‑value pairs.

## API

### AttributeAnalyzer()

**Purpose**  
Initializes a new instance of the `AttributeAnalyzer` class.

**Parameters**  
None.

**Return value**  
A new `AttributeAnalyzer` instance ready for use.

**Exceptions**  
None.

### IEnumerable<AttributeInfo> AnalyzeAttributes()

**Purpose**  
Enumerates all attributes associated with the target symbol that was supplied when the analyzer was constructed.

**Parameters**  
None.

**Return value**  
An `IEnumerable<AttributeInfo>` containing information about each attribute (such as its type, constructor arguments, and named arguments). Returns an empty enumeration if no attributes are present.

**Exceptions**  
- `InvalidOperationException` – thrown if the analyzer has not been properly initialized with a target symbol.

### bool HasAttribute()

**Purpose**  
Determines whether the target symbol has at least one attribute of the type that the analyzer was configured to look for.

**Parameters**  
None.

**Return value**  
`true` if the target symbol possesses the specified attribute; otherwise `false`.

**Exceptions**  
- `InvalidOperationException` – thrown if the analyzer has not been initialized with a target symbol.

### Dictionary<string, string>? GetAttributeParameters()

**Purpose**  
Retrieves the constructor and named‑argument values of the target attribute as a dictionary where the key is the argument name (or positional index for constructor arguments) and the value is the argument’s constant value expressed as a string.

**Parameters**  
None.

**Return value**  
A `Dictionary<string, string>` containing the attribute’s arguments, or `null` if the target symbol does not have the attribute or if any argument cannot be represented as a string.

**Exceptions**  
- `InvalidOperationException` – thrown if the analyzer has not been initialized with a target symbol.

## Usage

### Basic attribute detection

```csharp
using DotNetSourceGeneratorToolkit;
using Microsoft.CodeAnalysis;

// Assume `semanticModel` and `symbol` are obtained from a generator context.
var analyzer = new AttributeAnalyzer(symbol, semanticModel);
if (analyzer.HasAttribute())
{
    foreach (var attr in analyzer.AnalyzeAttributes())
    {
        // Process each attribute (e.g., log its type)
        Console.WriteLine($"Found attribute: {attr.AttributeClass?.Name}");
    }
}
```

### Extracting attribute parameters

```csharp
using DotNetSourceGeneratorToolkit;
using Microsoft.CodeAnalysis;

// `targetSymbol` is the symbol being inspected (e.g., a method or class).
var analyzer = new AttributeAnalyzer(targetSymbol, semanticModel);
var parameters = analyzer.GetAttributeParameters();
if (parameters != null)
{
    foreach (var kvp in parameters)
    {
        Console.WriteLine($"{kvp.Key} = {kvp.Value}");
    }
}
```

## Notes

- The analyzer must be supplied with a valid `ISymbol` and `SemanticModel` via its constructor; otherwise all member calls will throw `InvalidOperationException`.
- The returned `AttributeInfo` instances are immutable snapshots of the attribute data at the time of analysis; subsequent changes to the source code will not affect already‑returned instances.
- `GetAttributeParameters` attempts to convert attribute arguments to their constant string representation using `ConstantValue.ToString()`. Arguments that are not compile‑time constants (e.g., `typeof` expressions or array creations) result in a `null` return for the entire dictionary.
- The type does not maintain any mutable state after construction; therefore it is safe to call its members concurrently from multiple threads, provided the underlying `ISymbol` and `SemanticModel` instances are not mutated during those calls. However, typical source‑generator usage operates on a single thread per generation pass, so explicit thread‑safety measures are generally unnecessary.
