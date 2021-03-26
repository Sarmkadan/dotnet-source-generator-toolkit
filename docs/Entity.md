# Entity

Represents a metadata model for source-generated entities, capturing structural information required by code generators in the `dotnet-source-generator-toolkit` project. It encapsulates identity, type hierarchy, properties, attributes, interfaces, and validation state to drive template-based code production.

## API

### `public string Id`
A unique identifier for the entity, typically used as a stable key across generations. Must not be null or empty.

### `public string Name`
The name of the entity as it appears in generated code. Must be a valid C# identifier.

### `public string Namespace`
The namespace into which the generated entity will be emitted. Must be a valid C# namespace.

### `public string? Description`
Optional human-readable documentation or summary attached to the entity. May be null.

### `public string? TableName`
Optional database table name when targeting ORM code generation. If null, a default naming convention is applied. Must be a valid SQL identifier when not null.

### `public List<EntityProperty> Properties`
Collection of all declared properties on the entity. Never null; may be empty. Modifications affect generated output.

### `public List<string> Attributes`
List of attribute declarations (e.g., `[Serializable]`, `[JsonObject]`) to be applied to the generated type. Never null; may be empty.

### `public List<string> Interfaces`
List of interface names the generated entity should implement. Never null; may be empty. Each item must be a valid interface identifier.

### `public string? BaseClass`
Optional base class name for the generated entity. If null, the entity is assumed to derive from `object`. Must be a valid class identifier when not null.

### `public DateTime CreatedAt`
Timestamp marking when the entity metadata was first created. Immutable after construction.

### `public DateTime UpdatedAt`
Timestamp marking the last modification to the entity metadata. Updated automatically on structural changes.

### `public bool IsAbstract`
Indicates whether the generated entity should be marked `abstract`. Defaults to `false`.

### `public bool IsSealed`
Indicates whether the generated entity should be marked `sealed`. Defaults to `false`.

### `public AccessModifier AccessModifier`
Controls the visibility of the generated entity. Defaults to `AccessModifier.Public`.

### `public void AddProperty(EntityProperty property)`
Adds a new property to the entity. Throws `ArgumentNullException` if `property` is null. Updates `UpdatedAt` on success.

**Parameters**
- `property`: The property to add.

**Exceptions**
- `ArgumentNullException`: if `property` is null.

### `public bool RemoveProperty(string propertyName)`
Removes the property with the given name from the entity. Returns `true` if a property was removed; otherwise `false`. Updates `UpdatedAt` on success.

**Parameters**
- `propertyName`: Name of the property to remove.

**Return Value**
- `true` if the property existed and was removed; otherwise `false`.

### `public EntityProperty? GetPrimaryKeyProperty()`
Returns the first property marked as primary key, or `null` if none exists.

**Return Value**
- The primary key property, or `null`.

### `public IEnumerable<EntityProperty> GetNavigationProperties()`
Returns an enumerable of all properties marked as navigation (e.g., reference or collection types). Never null; may be empty.

**Return Value**
- Enumeration of navigation properties.

### `public IEnumerable<string> Validate()`
Validates the current state of the entity and returns a list of error messages. Checks include: non-empty `Id`, valid `Name`, valid `Namespace`, and consistency of `BaseClass` and `Interfaces`. Never throws.

**Return Value**
- Enumerable of validation error messages. Empty if valid.

## Usage
