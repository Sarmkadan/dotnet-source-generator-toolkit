# TypeExtensions
Provides a set of extension methods for `System.Type` that simplify common type‑introspection tasks such as checking nullability, numeric nature, simplicity, and assignability, as well as retrieving friendly names, default values, base type hierarchies, and collection detection.

## API
### IsNullable
```csharp
public static bool IsNullable(this Type type)
```
**Purpose:** Returns `true` if `type` represents a nullable value type (`Nullable<T>`) or a reference type; otherwise `false`.  
**Parameters:**  
- `type`: The type to inspect.  
**Return value:** `true` for nullable or reference types; `false` for non‑nullable value types.  
**Exceptions:** Throws `ArgumentNullException` if `type` is `null`.

### IsNumeric
```csharp
public static bool IsNumeric(this Type type)
```
**Purpose:** Determines whether `type` is a built‑in numeric type (integral or floating‑point) or `Nullable<T>` where `T` is numeric.  
**Parameters:**  
- `type`: The type to inspect.  
**Return value:** `true` for numeric types; `false` otherwise.  
**Exceptions:** Throws `ArgumentNullException` if `type` is `null`.

### IsSimpleType
```csharp
public static bool IsSimpleType(this Type type)
```
**Purpose:** Indicates whether `type` is considered a “simple” type: primitive types, `string`, `DateTime`, `DateTimeOffset`, `TimeSpan`, `Guid`, `decimal`, and their nullable equivalents.  
**Parameters:**  
- `type`: The type to inspect.  
**Return value:** `true` for simple types; `false` otherwise.  
**Exceptions:** Throws `ArgumentNullException` if `type` is `null`.

### GetFriendlyName
```csharp
public static string GetFriendlyName(this Type type)
```
**Purpose:** Produces a human‑readable representation of `type`, including generic argument names and nullable modifiers (e.g., `List<string>` or `int?`).  
**Parameters:**  
- `type`: The type to format.  
**Return value:** A string containing the friendly name.  
**Exceptions:** Throws `ArgumentNullException` if `type` is `null`.

### GetDefaultValue
```csharp
public static object? GetDefaultValue(this Type type)
```
**Purpose:** Returns the default value for `type` (`null` for reference types and nullable value types; zero‑filled value for structs).  
**Parameters:**  
- `type`: The type for which to obtain the default value.  
**Return value:** An instance representing the default value, or `null` if the type is a reference type or nullable.  
**Exceptions:**  
- `ArgumentNullException` if `type` is `null`.  
- `InvalidOperationException` if `type` is a value type that cannot be instantiated (e.g., an abstract struct or a type without a parameterless constructor).

### IsAssignableTo
```csharp
public static bool IsAssignableTo(this Type type, Type other)
```
**Purpose:** Determines whether instances of `type` can be assigned to a variable of type `other` (equivalent to `other.IsAssignableFrom(type)`).  
**Parameters:**  
- `type`: The source type.  
- `other`: The target type.  
**Return value:** `true` if `type` is assignable to `other`; otherwise `false`.  
**Exceptions:**  
- `ArgumentNullException` if either `type` or `other` is `null`.

### GetBaseTypes
```csharp
public static IEnumerable<Type> GetBaseTypes(this Type type)
```
**Purpose:** Enumerates the inheritance hierarchy of `type`, returning each base class (excluding `type` itself and `object`).  
**Parameters:**  
- `type`: The type whose base types are to be enumerated.  
**Return value:** An `IEnumerable<Type>` yielding each base class in order from immediate parent up to `System.Object` (exclusive).  
**Exceptions:** Throws `ArgumentNullException` if `type` is `null`.

### IsCollection
```csharp
public static bool IsCollection(this Type type)
```
**Purpose:** Returns `true` if `type` represents a collection type, defined as implementing `System.Collections.IEnumerable` and not being `string` or `byte[]`.  
**Parameters:**  
- `type`: The type to evaluate.  
**Return value:** `true` for collection types; `false` otherwise.  
**Exceptions:** Throws `ArgumentNullException` if `type` is `null`.

## Usage
```csharp
using System;
using System.Collections.Generic;

static class Demo
{
    static void Main()
    {
        Type t = typeof(List<int>);

        bool isCol = t.IsCollection();                 // true
        string name = t.GetFriendlyName();             // "List<System.Int32>"
        Type[] bases = t.GetBaseTypes().ToArray();    // [System.Object]
        Console.WriteLine($"{name} is collection: {isCol}");
    }
}
```

```csharp
using System;

static class Demo2
{
    static void Main()
    {
        Type? nullableInt = typeof(int?);
        Type? regularInt  = typeof(int);

        Console.WriteLine(nullableInt.IsNullable());   // True
        Console.WriteLine(regularInt.IsNullable());    // False
        Console.WriteLine(nullableInt.IsNumeric());    // True
        Console.WriteLine(regularInt.IsNumeric());     // True

        object? defInt  = regularInt.GetDefaultValue(); // 0
        object? defStr  = typeof(string).GetDefaultValue(); // null
        Console.WriteLine($"default(int) = {defInt}, default(string) = {defStr}");
    }
}
```

## Notes
- All methods are pure extensions; they rely only on the input `Type` and have no internal state, making them thread‑safe for concurrent use.  
- `IsNullable` treats reference types as nullable because they can hold `null`.  
- `IsSimpleType` does **not** consider user‑defined structs or enums as simple, even if they contain only primitive fields.  
- `GetDefaultValue` returns `null` for reference types and nullable value types; for non‑nullable value types it returns the result of `Activator.CreateInstance(type)`, which throws if the type lacks a parameterless constructor or is abstract.  
- `IsCollection` excludes `string` and `byte[]` despite their implementation of `IEnumerable` because they are typically treated as scalar values in collection‑aware APIs.  
- When dealing with open generic types (e.g., `typeof(List<>)`), the methods behave as defined by the underlying reflection APIs; callers should close the generic arguments if specific type information is required.  
- Passing `null` for the `type` argument (or `other` in `IsAssignableTo`) will always result in an `ArgumentNullException`.
