# EntityTests

`EntityTests` is a suite of unit tests designed to verify the core logic, data validation rules, and type resolution capabilities of entity-related components within the `dotnet-source-generator-toolkit`. The tests ensure that entity modeling, primary key identification, and validation logic function correctly under various conditions, including edge cases related to C# type mapping and attribute generation.

## API

### AddProperty_WhenDuplicatePropertyName_ThrowsInvalidOperationException
Verifies that attempting to add a property with a name that already exists within an entity model triggers an `InvalidOperationException`.

### GetPrimaryKeyProperty_WhenPrimaryKeyExists_ReturnsThatProperty
Confirms that the primary key detection mechanism correctly identifies and returns the property designated as the primary key when one is defined.

### Validate_WhenEntityNameIsEmpty_ReturnsNameRequiredError
Validates that the entity validation logic correctly identifies a missing entity name as an error condition and returns the appropriate error message or state.

### GetClrTypeName_WithNullableIntType_ReturnsNullableIntSuffix
Tests the type name resolution logic to ensure that nullable integer types (`int?`) are correctly mapped to their expected C# string representation.

### GetClrTypeName_WithCollectionStringType_ReturnsListGeneric
Verifies that collection types of strings are correctly resolved to the expected generic `List` or collection representation.

### GenerateValidationAttributes_WithRequiredAndMaxLength_ReturnsBothAttributes
Checks that the attribute generator correctly produces both `[Required]` and `[MaxLength]` validation attributes when specified in the entity metadata.

### AddError_WhenCalled_SetsStatusToFailedAndRecordsMessage
Ensures that the error handling mechanism correctly updates the entity status to failed and successfully stores the provided error message.

### IsSuccessful_WhenCompletedWithNoErrors_ReturnsTrue
Validates that the `IsSuccessful` property correctly reflects a successful state (returning `true`) when no errors have been recorded against the entity.

## Usage

```csharp
// Example 1: Verifying duplicate property handling
[Fact]
public void TestDuplicatePropertyThrows()
{
    var entity = new Entity();
    entity.AddProperty("Id", typeof(int));
    
    Assert.Throws<InvalidOperationException>(() => 
        entity.AddProperty("Id", typeof(string)));
}

// Example 2: Checking successful validation status
[Fact]
public void TestSuccessfulValidation()
{
    var entity = new Entity { Name = "User" };
    
    // Act
    entity.Validate();
    
    // Assert
    Assert.True(entity.IsSuccessful);
}
```

## Notes

*   **Edge Cases:** Tests cover scenarios involving empty strings for names, nullability in types, and handling of duplicate identifiers. It is assumed that components being tested do not perform validation on input types beyond standard reflection checks.
*   **Thread Safety:** The tests assume a single-threaded execution environment for individual test cases. The components being tested (`Entity` instances) are generally not designed for concurrent mutation; thread safety should be managed at a higher level if entities are shared across threads.
