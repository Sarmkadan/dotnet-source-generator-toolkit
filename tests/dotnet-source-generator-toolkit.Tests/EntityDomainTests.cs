#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNetSourceGeneratorToolkit.Domain;
using FluentAssertions;
using Xunit;

/// <summary>
/// Tests for the Entity class.
/// </summary>
public sealed class EntityTests
{
    /// <summary>
    /// Verifies that adding a duplicate property name throws an InvalidOperationException.
    /// </summary>
    [Fact]
    public void AddProperty_WhenDuplicatePropertyName_ThrowsInvalidOperationException()
    {
        // Arrange
        var entity = new Entity { Name = "Product", Namespace = "Shop.Domain" };
        var first = new EntityProperty { Name = "Price", Type = "decimal" };
        var duplicate = new EntityProperty { Name = "Price", Type = "int" };
        entity.AddProperty(first);

        // Act
        var act = () => entity.AddProperty(duplicate);

        // Assert
        act.Should().Throw<InvalidOperationException>()
           .WithMessage("*Price*");
    }

    /// <summary>
    /// Verifies that getting the primary key property returns the correct property when it exists.
    /// </summary>
    [Fact]
    public void GetPrimaryKeyProperty_WhenPrimaryKeyExists_ReturnsThatProperty()
    {
        // Arrange
        var entity = new Entity { Name = "Customer", Namespace = "Shop.Domain" };
        entity.AddProperty(new EntityProperty { Name = "Id", Type = "int", IsPrimaryKey = true });
        entity.AddProperty(new EntityProperty { Name = "Name", Type = "string" });

        // Act
        var pk = entity.GetPrimaryKeyProperty();

        // Assert
        pk.Should().NotBeNull();
        pk!.Name.Should().Be("Id");
        pk.IsPrimaryKey.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that validating an entity with an empty name returns a NameRequiredError.
    /// </summary>
    [Fact]
    public void Validate_WhenEntityNameIsEmpty_ReturnsNameRequiredError()
    {
        // Arrange
        var entity = new Entity { Name = "", Namespace = "Shop.Domain" };
        entity.AddProperty(new EntityProperty { Name = "Id", Type = "int" });

        // Act
        var errors = entity.Validate().ToList();

        // Assert
        errors.Should().Contain(e => e.Contains("name is required", StringComparison.OrdinalIgnoreCase));
    }
}

/// <summary>
/// Tests for the EntityProperty class.
/// </summary>
public sealed class EntityPropertyTests
{
    /// <summary>
    /// Verifies that getting the CLR type name for a nullable int type returns the correct type name.
    /// </summary>
    [Fact]
    public void GetClrTypeName_WithNullableIntType_ReturnsNullableIntSuffix()
    {
        // Arrange
        var property = new EntityProperty { Type = "int", IsNullable = true };

        // Act
        var typeName = property.GetClrTypeName();

        // Assert
        typeName.Should().Be("int?");
    }

    /// <summary>
    /// Verifies that getting the CLR type name for a collection string type returns the correct type name.
    /// </summary>
    [Fact]
    public void GetClrTypeName_WithCollectionStringType_ReturnsListGeneric()
    {
        // Arrange
        var property = new EntityProperty { Type = "string", IsCollection = true };

        // Act
        var typeName = property.GetClrTypeName();

        // Assert
        typeName.Should().Be("List<string>");
    }

    /// <summary>
    /// Verifies that generating validation attributes for a property with required and max length attributes returns both attributes.
    /// </summary>
    [Fact]
    public void GenerateValidationAttributes_WithRequiredAndMaxLength_ReturnsBothAttributes()
    {
        // Arrange
        var property = new EntityProperty
        {
            Name = "Email",
            Type = "string",
            IsRequired = true,
            MaxLength = 256,
        };

        // Act
        var attributes = property.GenerateValidationAttributes().ToList();

        // Assert
        attributes.Should().Contain("[Required]");
        attributes.Should().Contain("[MaxLength(256)]");
    }
}

/// <summary>
/// Tests for the GenerationResult class.
/// </summary>
public sealed class GenerationResultTests
{
    /// <summary>
    /// Verifies that adding an error to a generation result sets the status to failed and records the message.
    /// </summary>
    [Fact]
    public void AddError_WhenCalled_SetsStatusToFailedAndRecordsMessage()
    {
        // Arrange
        var result = new GenerationResult { EntityName = "Product" };

        // Act
        result.AddError("Code generation failed due to missing namespace");

        // Assert
        result.Status.Should().Be(GenerationStatus.Failed);
        result.Errors.Should().ContainSingle()
              .Which.Should().Be("Code generation failed due to missing namespace");
    }

    /// <summary>
    /// Verifies that a generation result is successful when completed with no errors.
    /// </summary>
    [Fact]
    public void IsSuccessful_WhenCompletedWithNoErrors_ReturnsTrue()
    {
        // Arrange
        var result = new GenerationResult
        {
            EntityName = "Order",
            GeneratedCode = "public class OrderRepository { }",
            OutputFilePath = "Repositories/OrderRepository.cs",
        };

        // Act
        result.MarkAsCompleted(GenerationStatus.Completed, 42);

        // Assert
        result.IsSuccessful().Should().BeTrue();
        result.GenerationDurationMs.Should().Be(42);
    }
}
