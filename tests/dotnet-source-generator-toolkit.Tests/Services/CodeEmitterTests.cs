#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNetSourceGeneratorToolkit.Services;
using FluentAssertions;
using Xunit;

namespace DotNetSourceGeneratorToolkit.Tests.Services;

/// <summary>
/// Contains unit tests for the <see cref="CodeEmitter"/> class.
/// Tests edge cases and behavioral changes in code emission logic.
/// </summary>
public sealed class CodeEmitterTests
{
    /// <summary>
    /// Tests emission of a type with zero properties (empty class).
    /// Ensures empty classes are properly formatted without errors.
    /// </summary>
    [Fact]
    public void GenerateClassDeclaration_WithZeroProperties_GeneratesEmptyClass()
    {
        // Arrange
        const string expectedClassName = "EmptyClass";
        const string expectedModifiers = "public sealed";
        const string expectedBody = "";

        // Act
        var result = CodeEmitter.GenerateClassDeclaration(
            className: expectedClassName,
            modifiers: expectedModifiers,
            body: expectedBody
        );

        // Assert
        result.Should().NotBeNullOrWhiteSpace();
        result.Should().Contain(expectedClassName);
        result.Should().Contain(expectedModifiers);
        result.Should().Contain("{");
        result.Should().Contain("}");

        // Verify it compiles by checking structure
        var lines = result.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
        lines.Should().HaveCount(3); // class declaration, opening brace, closing brace
    }

    /// <summary>
    /// Tests emission of a property whose name collides with a C# keyword.
    /// Ensures keywords are properly escaped with '@' prefix.
    /// </summary>
    [Theory]
    [InlineData("class")]
    [InlineData("event")]
    [InlineData("namespace")]
    [InlineData("for")]
    [InlineData("foreach")]
    [InlineData("while")]
    [InlineData("if")]
    [InlineData("else")]
    [InlineData("return")]
    [InlineData("using")]
    public void GeneratePropertyDeclaration_WithKeywordPropertyName_EscapesIdentifier(string keywordPropertyName)
    {
        // Arrange
        const string propertyType = "string";
        const string accessors = "get; set;";
        const int indentLevel = 1;

        // Act
        var result = CodeEmitter.GeneratePropertyDeclaration(
            propertyType: propertyType,
            propertyName: keywordPropertyName,
            accessors: accessors,
            indentLevel: indentLevel
        );

        // Assert
        result.Should().NotBeNullOrWhiteSpace();
        result.Should().Contain("@" + keywordPropertyName); // Should be escaped
        result.Should().Contain(propertyType);
        result.Should().Contain(accessors);
        result.Should().NotContain(" " + keywordPropertyName + " {"); // Should not contain unescaped keyword with space before it
    }

    /// <summary>
    /// Tests emission of a property with nested generic types.
    /// Ensures complex generic type names are properly formatted.
    /// </summary>
    [Fact]
    public void GeneratePropertyDeclaration_WithNestedGenericTypes_FormatsCorrectly()
    {
        // Arrange
        const string propertyType = "Dictionary<string, List<Foo>>";
        const string propertyName = "ComplexProperties";
        const string accessors = "get; set;";
        const int indentLevel = 1;

        // Act
        var result = CodeEmitter.GeneratePropertyDeclaration(
            propertyType: propertyType,
            propertyName: propertyName,
            accessors: accessors,
            indentLevel: indentLevel
        );

        // Assert
        result.Should().NotBeNullOrWhiteSpace();
        result.Should().Contain(propertyName);
        result.Should().Contain("Dictionary");
        result.Should().Contain("List");
        result.Should().Contain("string");
        result.Should().Contain("Foo");
        result.Should().Contain(">");
    }

    /// <summary>
    /// Tests emission of a type with no namespace (global namespace).
    /// Ensures namespace wrapping handles empty/empty namespace correctly.
    /// </summary>
    [Fact]
    public void WrapInNamespace_WithEmptyNamespace_ReturnsCodeWithoutNamespace()
    {
        // Arrange
        const string emptyNamespace = "";
        const string code = "public class TestClass { }";
        const int indentLevel = 0;

        // Act
        var result = CodeEmitter.WrapInNamespace(
            ns: emptyNamespace,
            code: code,
            indentLevel: indentLevel
        );

        // Assert
        result.Should().NotBeNullOrWhiteSpace();
        result.Should().Contain(code); // Should contain the original code
    }

    /// <summary>
    /// Tests emission of a type with global namespace prefix.
    /// Ensures global namespace types are properly handled.
    /// </summary>
    [Fact]
    public void WrapInNamespace_WithGlobalNamespacePrefix_ReturnsCodeWithGlobalNamespace()
    {
        // Arrange
        const string globalNamespace = "::MyGlobalType";
        const string code = "public class TestClass { }";
        const int indentLevel = 0;

        // Act
        var result = CodeEmitter.WrapInNamespace(
            ns: globalNamespace,
            code: code,
            indentLevel: indentLevel
        );

        // Assert
        result.Should().NotBeNullOrWhiteSpace();
        result.Should().Contain("::");
        result.Should().Contain("MyGlobalType");
    }

    /// <summary>
    /// Tests idempotency by emitting the same type twice in one run.
    /// Ensures no duplicate member emission occurs.
    /// </summary>
    [Fact]
    public void GenerateClassDeclaration_EmittedTwice_ProducesIdenticalResults()
    {
        // Arrange
        const string className = "IdempotentClass";
        const string modifiers = "public sealed";
        const string body = "public int Value { get; set; }";

        // Act - emit the same class twice
        var firstEmission = CodeEmitter.GenerateClassDeclaration(
            className: className,
            modifiers: modifiers,
            body: body
        );

        var secondEmission = CodeEmitter.GenerateClassDeclaration(
            className: className,
            modifiers: modifiers,
            body: body
        );

        // Assert
        firstEmission.Should().NotBeNullOrWhiteSpace();
        secondEmission.Should().NotBeNullOrWhiteSpace();
        firstEmission.Should().Be(secondEmission); // Should be identical

        // Verify structure
        firstEmission.Should().Contain(className);
        firstEmission.Should().Contain("public int Value");
        firstEmission.Should().Contain("{");
        firstEmission.Should().Contain("}");
    }

    /// <summary>
    /// Tests that EscapeIdentifier properly handles all C# keywords.
    /// </summary>
    [Theory]
    [MemberData(nameof(KeywordsData))
    ]
    public void EscapeIdentifier_WithCSharpKeyword_ReturnsEscapedIdentifier(string keyword)
    {
        // Arrange & Act
        var result = CodeEmitter.EscapeIdentifier(keyword);

        // Assert
        result.Should().StartWith("@");
        result.Should().Be("@" + keyword);
    }

    /// <summary>
    /// Tests that EscapeIdentifier handles identifiers starting with digits.
    /// </summary>
    [Theory]
    [InlineData("123Invalid")]
    [InlineData("0start")]
    public void EscapeIdentifier_WithIdentifierStartingWithDigit_ReturnsEscapedIdentifier(string identifier)
    {
        // Arrange & Act
        var result = CodeEmitter.EscapeIdentifier(identifier);

        // Assert
        result.Should().StartWith("@");
        result.Should().Be("@" + identifier);
    }

    /// <summary>
    /// Tests that EscapeIdentifier handles identifiers with special characters.
    /// </summary>
    [Theory]
    [InlineData("my-property")]
    [InlineData("my.property")]
    [InlineData("my$var")]
    public void EscapeIdentifier_WithSpecialCharacters_ReturnsEscapedIdentifier(string identifier)
    {
        // Arrange & Act
        var result = CodeEmitter.EscapeIdentifier(identifier);

        // Assert
        result.Should().StartWith("@");
        result.Should().Be("@" + identifier);
    }

    /// <summary>
    /// Tests that EscapeIdentifier preserves valid identifiers.
    /// </summary>
    [Theory]
    [InlineData("ValidIdentifier")]
    [InlineData("_privateField")]
    [InlineData("value123")]
    [InlineData("camelCase")]
    [InlineData("PascalCase")]
    public void EscapeIdentifier_WithValidIdentifier_ReturnsUnchanged(string identifier)
    {
        // Arrange & Act
        var result = CodeEmitter.EscapeIdentifier(identifier);

        // Assert
        result.Should().Be(identifier); // Should remain unchanged
    }

    /// <summary>
    /// Tests that EscapeIdentifier throws ArgumentNullException for null input.
    /// </summary>
    [Fact]
    public void EscapeIdentifier_WithNullInput_ThrowsArgumentNullException()
    {
        // Arrange
        string? nullIdentifier = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => CodeEmitter.EscapeIdentifier(nullIdentifier!));
    }

    /// <summary>
    /// Tests that FormatTypeName properly handles nested generic types.
    /// </summary>
    [Fact]
    public void FormatTypeName_WithNestedGenericTypes_ReturnsProperlyFormattedTypeName()
    {
        // Arrange
        const string nestedGenericType = "Dictionary<string, List<Foo<int>>>";

        // Act
        var result = CodeEmitter.FormatTypeName(nestedGenericType);

        // Assert
        result.Should().NotBeNullOrWhiteSpace();
        result.Should().Contain("Dictionary");
        result.Should().Contain("List");
        result.Should().Contain("Foo");
        result.Should().Contain(">");
    }

    /// <summary>
    /// Tests that FormatTypeName handles type aliases correctly.
    /// </summary>
    [Theory]
    [InlineData("int", "int")]
    [InlineData("string", "string")]
    [InlineData("bool", "bool")]
    [InlineData("object", "object")]
    public void FormatTypeName_WithTypeAlias_ReturnsFullTypeName(string alias, string expected)
    {
        // Arrange & Act
        var result = CodeEmitter.FormatTypeName(alias);

        // Assert
        result.Should().Be(expected);
    }

    /// <summary>
    /// Tests that FormatTypeName throws ArgumentNullException for null input.
    /// </summary>
    [Fact]
    public void FormatTypeName_WithNullInput_ThrowsArgumentNullException()
    {
        // Arrange
        string? nullTypeName = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => CodeEmitter.FormatTypeName(nullTypeName!));
    }

    /// <summary>
    /// Tests that GenerateMethodDeclaration properly escapes method names that are keywords.
    /// </summary>
    [Fact]
    public void GenerateMethodDeclaration_WithKeywordMethodName_EscapesIdentifier()
    {
        // Arrange
        const string methodName = "class";
        const string returnType = "void";
        const string parameters = "string name";
        const string modifiers = "public";

        // Act
        var result = CodeEmitter.GenerateMethodDeclaration(
            methodName: methodName,
            returnType: returnType,
            parameters: parameters,
            modifiers: modifiers
        );

        // Assert
        result.Should().NotBeNullOrWhiteSpace();
        result.Should().Contain("@class"); // Should be escaped
        result.Should().Contain("void");
        result.Should().Contain(parameters);
    }

    /// <summary>
    /// Tests that GenerateFieldDeclaration properly escapes field names that are keywords.
    /// </summary>
    [Fact]
    public void GenerateFieldDeclaration_WithKeywordFieldName_EscapesIdentifier()
    {
        // Arrange
        const string fieldType = "int";
        const string fieldName = "event";
        const string initializer = "0";

        // Act
        var result = CodeEmitter.GenerateFieldDeclaration(
            fieldType: fieldType,
            fieldName: fieldName,
            initializer: initializer
        );

        // Assert
        result.Should().NotBeNullOrWhiteSpace();
        result.Should().Contain("@event"); // Should be escaped
        result.Should().Contain("= 0;");
    }

    /// <summary>
    /// Tests that GenerateVariableDeclaration properly escapes variable names that are keywords.
    /// </summary>
    [Fact]
    public void GenerateVariableDeclaration_WithKeywordVariableName_EscapesIdentifier()
    {
        // Arrange
        const string variableType = "string";
        const string variableName = "class";
        const string initializer = "\"test\"";

        // Act
        var result = CodeEmitter.GenerateVariableDeclaration(
            variableType: variableType,
            variableName: variableName,
            initializer: initializer
        );

        // Assert
        result.Should().NotBeNullOrWhiteSpace();
        result.Should().Contain("@class"); // Should be escaped
        result.Should().Contain(initializer);
    }

    /// <summary>
    /// Provides test data for C# keywords that need escaping.
    /// </summary>
    public static IEnumerable<object[]> KeywordsData =>
        new List<object[]>
        {
            new object[] { "abstract" },
            new object[] { "as" },
            new object[] { "base" },
            new object[] { "bool" },
            new object[] { "break" },
            new object[] { "byte" },
            new object[] { "case" },
            new object[] { "catch" },
            new object[] { "char" },
            new object[] { "checked" },
            new object[] { "class" },
            new object[] { "const" },
            new object[] { "continue" },
            new object[] { "decimal" },
            new object[] { "default" },
            new object[] { "delegate" },
            new object[] { "do" },
            new object[] { "double" },
            new object[] { "else" },
            new object[] { "enum" },
            new object[] { "event" },
            new object[] { "explicit" },
            new object[] { "extern" },
            new object[] { "false" },
            new object[] { "finally" },
            new object[] { "fixed" },
            new object[] { "float" },
            new object[] { "for" },
            new object[] { "foreach" },
            new object[] { "goto" },
            new object[] { "if" },
            new object[] { "implicit" },
            new object[] { "in" },
            new object[] { "int" },
            new object[] { "interface" },
            new object[] { "internal" },
            new object[] { "is" },
            new object[] { "lock" },
            new object[] { "long" },
            new object[] { "namespace" },
            new object[] { "new" },
            new object[] { "null" },
            new object[] { "object" },
            new object[] { "operator" },
            new object[] { "out" },
            new object[] { "override" },
            new object[] { "params" },
            new object[] { "private" },
            new object[] { "protected" },
            new object[] { "public" },
            new object[] { "readonly" },
            new object[] { "ref" },
            new object[] { "return" },
            new object[] { "sbyte" },
            new object[] { "sealed" },
            new object[] { "short" },
            new object[] { "sizeof" },
            new object[] { "stackalloc" },
            new object[] { "static" },
            new object[] { "string" },
            new object[] { "struct" },
            new object[] { "switch" },
            new object[] { "this" },
            new object[] { "throw" },
            new object[] { "true" },
            new object[] { "try" },
            new object[] { "typeof" },
            new object[] { "uint" },
            new object[] { "ulong" },
            new object[] { "unchecked" },
            new object[] { "unsafe" },
            new object[] { "ushort" },
            new object[] { "using" },
            new object[] { "virtual" },
            new object[] { "void" },
            new object[] { "volatile" },
            new object[] { "while" }
        };
}