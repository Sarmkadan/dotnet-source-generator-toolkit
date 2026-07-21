#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using DotNetSourceGeneratorToolkit.Infrastructure;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

/// <summary>
/// Tests for the AttributeAnalyzer class.
/// </summary>
public sealed class AttributeAnalyzerTests
{
    private readonly Mock<ILogger<AttributeAnalyzer>> _mockLogger;
    private readonly AttributeAnalyzer _analyzer;

    /// <summary>
    /// Initializes a new instance of the <see cref="AttributeAnalyzerTests"/> class.
    /// </summary>
    public AttributeAnalyzerTests()
    {
        _mockLogger = new Mock<ILogger<AttributeAnalyzer>>();
        _analyzer = new AttributeAnalyzer(_mockLogger.Object);
    }

    /// <summary>
    /// Verifies that analyzing null source code returns an empty list of attributes.
    /// </summary>
    [Fact]
    public void AnalyzeAttributes_WithNullSourceCode_ReturnsEmptyList()
    {
        // Arrange & Act
        var result = _analyzer.AnalyzeAttributes(null!);

        // Assert
        result.Should().BeEmpty();
    }

    /// <summary>
    /// Verifies that analyzing empty source code returns an empty list of attributes.
    /// </summary>
    [Fact]
    public void AnalyzeAttributes_WithEmptySourceCode_ReturnsEmptyList()
    {
        // Arrange & Act
        var result = _analyzer.AnalyzeAttributes(string.Empty);

        // Assert
        result.Should().BeEmpty();
    }

    /// <summary>
    /// Verifies that analyzing source code without attributes returns an empty list.
    /// </summary>
    [Fact]
    public void AnalyzeAttributes_WithSourceCodeWithoutAttributes_ReturnsEmptyList()
    {
        // Arrange
        var sourceCode = """
        public class MyClass
        {
            public void MyMethod() { }
        }
        """;

        // Act
        var result = _analyzer.AnalyzeAttributes(sourceCode);

        // Assert
        result.Should().BeEmpty();
    }

    /// <summary>
    /// Verifies that analyzing source code with a single attribute returns the correct attribute info.
    /// </summary>
    [Fact]
    public void AnalyzeAttributes_WithSingleAttribute_ReturnsCorrectAttributeInfo()
    {
        // Arrange
        var sourceCode = """
        [Serializable]
        public class MyClass { }
        """;

        // Act
        var result = _analyzer.AnalyzeAttributes(sourceCode);

        // Assert
        result.Should().HaveCount(1);
        var attribute = result.First();
        attribute.Name.Should().Be("Serializable");
        attribute.Parameters.Should().BeEmpty();
    }

    /// <summary>
    /// Verifies that analyzing source code with multiple attributes returns all attributes.
    /// </summary>
    [Fact]
    public void AnalyzeAttributes_WithMultipleAttributes_ReturnsAllAttributes()
    {
        // Arrange
        var sourceCode = """
        [Serializable]
        [Obsolete("Use new class instead")]
        [CustomAttribute(Name = "Test", Value = 42)]
        public class MyClass { }
        """;

        // Act
        var result = _analyzer.AnalyzeAttributes(sourceCode);

        // Assert
        result.Should().HaveCount(3);
        result.Select(a => a.Name).Should().BeEquivalentTo(new[] { "Serializable", "Obsolete", "CustomAttribute" });
    }

    /// <summary>
    /// Verifies that analyzing source code with attribute containing named arguments returns correct parameters.
    /// </summary>
    [Fact]
    public void AnalyzeAttributes_WithAttributeNamedArguments_ReturnsCorrectParameters()
    {
        // Arrange
        var sourceCode = """
        [CustomAttribute(Name = "TestValue", Value = 42, Enabled = true)]
        public class MyClass { }
        """;

        // Act
        var result = _analyzer.AnalyzeAttributes(sourceCode);

        // Assert
        result.Should().HaveCount(1);
        var attribute = result.First();
        attribute.Name.Should().Be("CustomAttribute");
        attribute.Parameters.Should().HaveCount(3);
        attribute.Parameters["Name"].Should().Be("TestValue");
        attribute.Parameters["Value"].Should().Be("42");
        attribute.Parameters["Enabled"].Should().Be("true");
    }

    /// <summary>
    /// Verifies that analyzing source code with attribute containing positional arguments returns empty parameters.
    /// </summary>
    [Fact]
    public void AnalyzeAttributes_WithAttributePositionalArguments_ReturnsEmptyParameters()
    {
        // Arrange
        var sourceCode = """
        [Obsolete("Use new class instead")]
        public class MyClass { }
        """;

        // Act
        var result = _analyzer.AnalyzeAttributes(sourceCode);

        // Assert
        result.Should().HaveCount(1);
        var attribute = result.First();
        attribute.Name.Should().Be("Obsolete");
        attribute.Parameters.Should().BeEmpty();
    }

    /// <summary>
    /// Verifies that HasAttribute returns true when attribute is present.
    /// </summary>
    [Fact]
    public void HasAttribute_WithExistingAttribute_ReturnsTrue()
    {
        // Arrange
        var sourceCode = """
        [Serializable]
        public class MyClass { }
        """;

        // Act
        var result = _analyzer.HasAttribute(sourceCode, "Serializable");

        // Assert
        result.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that HasAttribute returns false when attribute is not present.
    /// </summary>
    [Fact]
    public void HasAttribute_WithNonExistingAttribute_ReturnsFalse()
    {
        // Arrange
        var sourceCode = """
        [Serializable]
        public class MyClass { }
        """;

        // Act
        var result = _analyzer.HasAttribute(sourceCode, "Obsolete");

        // Assert
        result.Should().BeFalse();
    }

    /// <summary>
    /// Verifies that HasAttribute returns false when source code is null.
    /// </summary>
    [Fact]
    public void HasAttribute_WithNullSourceCode_ReturnsFalse()
    {
        // Arrange & Act
        var result = _analyzer.HasAttribute(null!, "Serializable");

        // Assert
        result.Should().BeFalse();
    }

    /// <summary>
    /// Verifies that HasAttribute returns false when attribute name is null.
    /// </summary>
    [Fact]
    public void HasAttribute_WithNullAttributeName_ReturnsFalse()
    {
        // Arrange
        var sourceCode = """
        [Serializable]
        public class MyClass { }
        """;

        // Act
        var result = _analyzer.HasAttribute(sourceCode, null!);

        // Assert
        result.Should().BeFalse();
    }

    /// <summary>
    /// Verifies that GetAttributeParameters returns correct parameters for existing attribute.
    /// </summary>
    [Fact]
    public void GetAttributeParameters_WithExistingAttribute_ReturnsParameters()
    {
        // Arrange
        var sourceCode = """
        [CustomAttribute(Name = "Test", Value = 123, Enabled = true)]
        public class MyClass { }
        """;

        // Act
        var result = _analyzer.GetAttributeParameters(sourceCode, "CustomAttribute");

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(3);
        result!["Name"].Should().Be("Test");
        result["Value"].Should().Be("123");
        result["Enabled"].Should().Be("true");
    }

    /// <summary>
    /// Verifies that GetAttributeParameters returns null when attribute is not present.
    /// </summary>
    [Fact]
    public void GetAttributeParameters_WithNonExistingAttribute_ReturnsNull()
    {
        // Arrange
        var sourceCode = """
        [Serializable]
        public class MyClass { }
        """;

        // Act
        var result = _analyzer.GetAttributeParameters(sourceCode, "CustomAttribute");

        // Assert
        result.Should().BeNull();
    }

    /// <summary>
    /// Verifies that GetAttributeParameters returns null when source code is null.
    /// </summary>
    [Fact]
    public void GetAttributeParameters_WithNullSourceCode_ReturnsNull()
    {
        // Arrange & Act
        var result = _analyzer.GetAttributeParameters(null!, "Serializable");

        // Assert
        result.Should().BeNull();
    }

    /// <summary>
    /// Verifies that GetAttributeParameters returns null when attribute name is null.
    /// </summary>
    [Fact]
    public void GetAttributeParameters_WithNullAttributeName_ReturnsNull()
    {
        // Arrange
        var sourceCode = """
        [Serializable]
        public class MyClass { }
        """;

        // Act
        var result = _analyzer.GetAttributeParameters(sourceCode, null!);

        // Assert
        result.Should().BeNull();
    }

    /// <summary>
    /// Verifies that attribute name parsing is case-insensitive.
    /// </summary>
    [Fact]
    public void AnalyzeAttributes_WithCaseInsensitiveAttributeName_ParsesCorrectly()
    {
        // Arrange
        var sourceCode = """
        [serializable]
        public class MyClass { }
        """;

        // Act
        var result = _analyzer.AnalyzeAttributes(sourceCode);

        // Assert
        result.Should().HaveCount(1);
        result.First().Name.Should().Be("serializable");
    }

    /// <summary>
    /// Verifies that attribute with whitespace around name is parsed correctly.
    /// </summary>
    [Fact]
    public void AnalyzeAttributes_WithWhitespaceAroundAttributeName_ParsesCorrectly()
    {
        // Arrange
        var sourceCode = """
        [  Serializable  ]
        public class MyClass { }
        """;

        // Act
        var result = _analyzer.AnalyzeAttributes(sourceCode);

        // Assert
        result.Should().HaveCount(1);
        result.First().Name.Should().Be("Serializable");
    }

    /// <summary>
    /// Verifies that attribute with complex parameter values is parsed correctly.
    /// </summary>
    [Fact]
    public void AnalyzeAttributes_WithComplexParameterValues_ParsesCorrectly()
    {
        // Arrange
        var sourceCode = """
        [CustomAttribute(Name = "Test Value", Description = "A test description", Count = 42, Enabled = false)]
        public class MyClass { }
        """;

        // Act
        var result = _analyzer.AnalyzeAttributes(sourceCode);

        // Assert
        result.Should().HaveCount(1);
        var attribute = result.First();
        attribute.Parameters.Should().HaveCount(4);
        attribute.Parameters["Name"].Should().Be("Test Value");
        attribute.Parameters["Description"].Should().Be("A test description");
        attribute.Parameters["Count"].Should().Be("42");
        attribute.Parameters["Enabled"].Should().Be("false");
    }

    /// <summary>
    /// Verifies that multiple attributes with same name are all detected.
    /// </summary>
    [Fact]
    public void AnalyzeAttributes_WithMultipleSameAttributes_ReturnsAllInstances()
    {
        // Arrange
        var sourceCode = """
        [Serializable]
        [Serializable]
        public class MyClass { }
        """;

        // Act
        var result = _analyzer.AnalyzeAttributes(sourceCode);

        // Assert
        result.Should().HaveCount(2);
        result.Count(a => a.Name == "Serializable").Should().Be(2);
    }

    /// <summary>
    /// Verifies that attribute with qualified name (namespace) is parsed correctly.
    /// </summary>
    [Fact]
    public void AnalyzeAttributes_WithQualifiedAttributeName_ParsesCorrectly()
    {
        // Arrange
        var sourceCode = """
        [System.Serializable]
        public class MyClass { }
        """;

        // Act
        var result = _analyzer.AnalyzeAttributes(sourceCode);

        // Assert
        result.Should().HaveCount(1);
        result.First().Name.Should().Be("System.Serializable");
    }
}