// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNetSourceGeneratorToolkit.Utilities;
using FluentAssertions;

namespace DotNetSourceGeneratorToolkit.Tests;

public class StringExtensionsTests
{
    [Fact]
    public void ToPascalCase_WithUnderscoreDelimiters_ReturnsPascalCase()
    {
        // Arrange
        var input = "hello_world_test";

        // Act
        var result = input.ToPascalCase();

        // Assert
        result.Should().Be("HelloWorldTest");
    }

    [Fact]
    public void ToCamelCase_WithMultipleWords_ReturnsFirstWordLowercased()
    {
        // Arrange
        var input = "my_property_name";

        // Act
        var result = input.ToCamelCase();

        // Assert
        result.Should().Be("myPropertyName");
    }

    [Fact]
    public void ToSnakeCase_WithPascalCaseString_InsertsUnderscoresBetweenWords()
    {
        // Arrange
        var input = "MyEntityName";

        // Act
        var result = input.ToSnakeCase();

        // Assert
        result.Should().Be("my_entity_name");
    }

    [Fact]
    public void Truncate_WhenStringExceedsMaxLength_AppendsEllipsis()
    {
        // Arrange
        var input = "This is a very long string";

        // Act
        var result = input.Truncate(10);

        // Assert
        result.Should().StartWith("This is a ");
        result.Should().EndWith("...");
        result.Length.Should().Be(13);
    }
}

public class StringValidatorTests
{
    [Fact]
    public void IsValidIdentifier_WithValidCSharpIdentifier_ReturnsTrue()
    {
        // Arrange & Act & Assert
        StringValidator.IsValidIdentifier("MyEntityClass").Should().BeTrue();
        StringValidator.IsValidIdentifier("_privateField").Should().BeTrue();
        StringValidator.IsValidIdentifier("value123").Should().BeTrue();
    }

    [Fact]
    public void IsValidIdentifier_WhenStartsWithDigit_ReturnsFalse()
    {
        // Arrange & Act & Assert
        StringValidator.IsValidIdentifier("123abc").Should().BeFalse();
        StringValidator.IsValidIdentifier("1invalid").Should().BeFalse();
    }

    [Fact]
    public void IsValidNamespace_WithDotSeparatedIdentifiers_ReturnsTrue()
    {
        // Arrange & Act & Assert
        StringValidator.IsValidNamespace("My.Project.Domain").Should().BeTrue();
        StringValidator.IsValidNamespace("DotNetSourceGeneratorToolkit.Services").Should().BeTrue();
    }

    [Fact]
    public void SanitizeForFileName_WithNullOrWhiteSpaceValue_ReturnsUnnamedFallback()
    {
        // Arrange & Act & Assert
        StringValidator.SanitizeForFileName("   ").Should().Be("unnamed");
        StringValidator.SanitizeForFileName("").Should().Be("unnamed");
    }
}
