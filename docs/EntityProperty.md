# EntityProperty
The `EntityProperty` type represents a property of an entity in a data model, providing metadata about the property such as its name, type, and constraints. This type is used to describe the structure and characteristics of data entities, allowing for more informed and robust data processing and manipulation.

## API
The `EntityProperty` type has the following public members:
* `Id`: A unique identifier for the property.
* `Name`: The name of the property.
* `Type`: The data type of the property.
* `ColumnName`: The name of the column in the underlying data storage, if applicable.
* `MaxLength` and `MinLength`: The maximum and minimum lengths of the property's value, if applicable.
* `RegexPattern`: A regular expression pattern that the property's value must match, if applicable.
* `MinValue` and `MaxValue`: The minimum and maximum values of the property, if applicable.
* `IsRequired`: A boolean indicating whether the property is required to have a value.
* `IsNullable`: A boolean indicating whether the property can have a null value.
* `IsPrimaryKey`: A boolean indicating whether the property is a primary key.
* `IsAutoIncrement`: A boolean indicating whether the property's value is automatically incremented.
* `IsNavigationProperty`: A boolean indicating whether the property is a navigation property.
* `IsCollection`: A boolean indicating whether the property is a collection.
* `DefaultValue`: The default value of the property, if applicable.
* `Description`: A description of the property.
* `Attributes`: A list of attributes associated with the property.
* `GetterAccess` and `SetterAccess`: The access modifiers for the property's getter and setter, respectively.

## Usage
Here are two examples of using the `EntityProperty` type:
```csharp
// Example 1: Creating an EntityProperty instance
var property = new EntityProperty
{
    Id = "Property1",
    Name = "Name",
    Type = "string",
    IsRequired = true,
    MaxLength = 50
};

// Example 2: Using EntityProperty to validate data
var entityProperty = new EntityProperty
{
    Id = "Property2",
    Name = "Age",
    Type = "int",
    MinValue = 18,
    MaxValue = 100
};

if (entityProperty.MinValue <= 25 && 25 <= entityProperty.MaxValue)
{
    Console.WriteLine("The value 25 is within the valid range for Property2.");
}
```

## Notes
When using the `EntityProperty` type, note that the `ColumnName` property may be null if the property is not backed by a column in a data storage. Additionally, the `RegexPattern` property may be null if no regular expression pattern is defined for the property. The `GetterAccess` and `SetterAccess` properties determine the accessibility of the property's getter and setter, respectively. The `EntityProperty` type is thread-safe, as it is an immutable type. However, when using instances of this type in a multithreaded environment, be aware that concurrent modifications to the `Attributes` list may result in unexpected behavior.
