# ReflectionHelper

Utility class providing common reflection operations for analyzing types, retrieving members, creating instances, and checking type relationships at runtime.

## API

### `public static IEnumerable<PropertyInfo> GetPublicProperties(Type type)`

Returns all public instance properties (including those declared on base types) of the specified type.

- **Parameters**
  - `type`: The `Type` to inspect.
- **Returns**
  - An `IEnumerable<PropertyInfo>` containing all public instance properties, ordered by inheritance (base types first).
- **Exceptions**
  - Throws `ArgumentNullException` if `type` is `null`.

---

### `public static IEnumerable<MethodInfo> GetPublicMethods(Type type)`

Returns all public instance methods (including those declared on base types) of the specified type.

- **Parameters**
  - `type`: The `Type` to inspect.
- **Returns**
  - An `IEnumerable<MethodInfo>` containing all public instance methods, ordered by inheritance (base types first).
- **Exceptions**
  - Throws `ArgumentNullException` if `type` is `null`.

---

### `public static bool ImplementsInterface(Type type, Type interfaceType)`

Determines whether the specified type implements the given interface.

- **Parameters**
  - `type`: The `Type` to check.
  - `interfaceType`: The `Type` representing the interface to verify.
- **Returns**
  - `true` if `type` implements `interfaceType`; otherwise, `false`.
- **Exceptions**
  - Throws `ArgumentNullException` if either `type` or `interfaceType` is `null`.
  - Throws `ArgumentException` if `interfaceType` is not an interface.

---

### `public static IEnumerable<Type> GetImplementations(Type interfaceType)`

Returns all types in the current application domain that implement the specified interface.

- **Parameters**
  - `interfaceType`: The `Type` representing the interface to search for.
- **Returns**
  - An `IEnumerable<Type>` of all loaded types that implement `interfaceType`.
- **Exceptions**
  - Throws `ArgumentNullException` if `interfaceType` is `null`.
  - Throws `ArgumentException` if `interfaceType` is not an interface.

---

### `public static object? CreateInstance(Type type)`

Creates an instance of the specified type using its parameterless constructor.

- **Parameters**
  - `type`: The `Type` to instantiate.
- **Returns**
  - A new instance of `type`; `null` if `type` is a value type or a type without a public parameterless constructor.
- **Exceptions**
  - Throws `ArgumentNullException` if `type` is `null`.
  - Throws `TargetInvocationException` if the constructor throws an exception.

---

### `public static IEnumerable<T> GetCustomAttributes<T>(MemberInfo memberInfo, bool inherit = true)`

Retrieves all custom attributes of the specified type applied to the given member.

- **Parameters**
  - `memberInfo`: The `MemberInfo` (e.g., `Type`, `MethodInfo`) to inspect.
  - `inherit`: If `true`, includes attributes inherited from base classes or interfaces.
- **Returns**
  - An `IEnumerable<T>` of custom attributes of type `T` applied to `memberInfo`.
- **Exceptions**
  - Throws `ArgumentNullException` if `memberInfo` is `null`.

---

### `public static IEnumerable<Type> GetBaseTypes(Type type)`

Returns all base types of the specified type, excluding `object` and `ValueType`, in ascending order from the immediate base to the root.

- **Parameters**
  - `type`: The `Type` to inspect.
- **Returns**
  - An `IEnumerable<Type>` of base types, ordered from immediate parent to root.
- **Exceptions**
  - Throws `ArgumentNullException` if `type` is `null`.

---
### `public static bool IsAutoProperty(PropertyInfo propertyInfo)`

Determines whether the specified property is an auto-implemented property (i.e., has a compiler-generated backing field).

- **Parameters**
  - `propertyInfo`: The `PropertyInfo` to check.
- **Returns**
  - `true` if the property is auto-implemented; otherwise, `false`.
- **Exceptions**
  - Throws `ArgumentNullException` if `propertyInfo` is `null`.

## Usage

```csharp
// Example 1: Discovering interface implementations
var interfaceType = typeof(IDisposable);
var implementations = ReflectionHelper.GetImplementations(interfaceType);
foreach (var impl in implementations)
{
    Console.WriteLine($"Type '{impl.FullName}' implements IDisposable.");
}

// Example 2: Checking for auto-properties
var myType = typeof(MyClass);
foreach (var prop in ReflectionHelper.GetPublicProperties(myType))
{
    if (ReflectionHelper.IsAutoProperty(prop))
    {
        Console.WriteLine($"Auto-property: {prop.Name}");
    }
}
```

## Notes

- **Thread Safety**: All methods are thread-safe and may be called concurrently from multiple threads.
- **Performance**: Reflection operations are inherently slower than direct code. Cache results where possible.
- **Edge Cases**:
  - `CreateInstance` returns `null` for value types (e.g., `int`, `struct`) or types without a public parameterless constructor.
  - `IsAutoProperty` returns `false` for properties with explicit backing fields or those defined in interfaces.
  - `GetImplementations` only returns types loaded in the current `AppDomain`; it does not scan assemblies dynamically.
  - `GetBaseTypes` excludes `object` and `ValueType` to avoid redundant results in typical inheritance chains.
