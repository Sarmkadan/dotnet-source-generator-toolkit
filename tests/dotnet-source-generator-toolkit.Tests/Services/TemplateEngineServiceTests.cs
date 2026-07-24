#nullable enable

// =============================================================================
// Author: Test Generator
// Tests for TemplateEngineService error handling and edge cases
// =============================================================================

using DotNetSourceGeneratorToolkit.Domain;
using DotNetSourceGeneratorToolkit.Exceptions;
using DotNetSourceGeneratorToolkit.Infrastructure;
using DotNetSourceGeneratorToolkit.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace DotNetSourceGeneratorToolkit.Tests.Services;

/// <summary>
/// Tests for <see cref="TemplateEngineService"/> covering error handling, malformed templates,
/// and edge cases for missing variables and unbalanced delimiters.
/// </summary>
public sealed class TemplateEngineServiceTests
{
    private static TemplateEngineService CreateService() =>
        new(new FileSystemService(NullLogger<FileSystemService>.Instance), NullLogger<TemplateEngineService>.Instance);

    private static Dictionary<string, object> CreateContext() => new() { ["Name"] = "TestEntity" };

    /// <summary>
    /// Test that referencing a variable that is never supplied throws a clear exception
    /// rather than silently emitting the literal placeholder.
    /// </summary>
    [Fact]
    public async Task RenderAsync_MissingVariable_ThrowsClearExceptionInsteadOfSilentPlaceholder()
    {
        // Arrange
        var service = CreateService();
        var template = "public class {{MissingVariable}} { }";
        var context = CreateContext(); // MissingVariable is not in context

        // Act & Assert
        var exception = await Assert.ThrowsAsync<GenerationException>(() => service.RenderAsync(template, context));
        Assert.Contains("Error rendering template", exception.Message);
    }

    /// <summary>
    /// Test that a template with mismatched/unbalanced placeholder delimiters throws a clear exception.
    /// </summary>
    [Fact]
    public async Task RenderAsync_MismatchedPlaceholderDelimiters_ThrowsClearException()
    {
        // Arrange
        var service = CreateService();
        var template = "public class {{MissingClosingTag { }"; // Missing closing }}
        var context = CreateContext();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<GenerationException>(() => service.RenderAsync(template, context));
        Assert.Contains("Error rendering template", exception.Message);
    }

    /// <summary>
    /// Test that an empty template string is handled gracefully.
    /// </summary>
    [Fact]
    public async Task RenderAsync_EmptyTemplate_ReturnsEmptyString()
    {
        // Arrange
        var service = CreateService();
        var template = string.Empty;
        var context = CreateContext();

        // Act
        var result = await service.RenderAsync(template, context);

        // Assert
        Assert.Equal(string.Empty, result);
    }

    /// <summary>
    /// Test that a template containing the same variable twice is handled correctly.
    /// </summary>
    [Fact]
    public async Task RenderAsync_SameVariableTwice_ReplacesAllOccurrences()
    {
        // Arrange
        var service = CreateService();
        var template = "{{Name}} is {{Name}}";
        var context = CreateContext();

        // Act
        var result = await service.RenderAsync(template, context);

        // Assert
        Assert.Equal("TestEntity is TestEntity", result);
    }

    /// <summary>
    /// Test that whitespace-only template input is handled gracefully.
    /// </summary>
    [Fact]
    public async Task RenderAsync_WhitespaceOnlyTemplate_ReturnsWhitespace()
    {
        // Arrange
        var service = CreateService();
        var template = "   \n\t  ";
        var context = CreateContext();

        // Act
        var result = await service.RenderAsync(template, context);

        // Assert
        Assert.Equal("   \n\t  ", result);
    }

    /// <summary>
    /// Test that missing variable in conditional block throws exception.
    /// </summary>
    [Fact]
    public async Task RenderAsync_MissingVariableInConditionalBlock_ThrowsClearException()
    {
        // Arrange
        var service = CreateService();
        var template = "{{#if MissingCondition}}public int Id { get; set; }{{/if}}";
        var context = CreateContext(); // MissingCondition is not in context

        // Act & Assert
        var exception = await Assert.ThrowsAsync<GenerationException>(() => service.RenderAsync(template, context));
        Assert.Contains("Error rendering template", exception.Message);
    }

    /// <summary>
    /// Test that missing variable in loop content throws exception.
    /// </summary>
    [Fact]
    public async Task RenderAsync_MissingVariableInLoopContent_ThrowsClearException()
    {
        // Arrange
        var service = CreateService();
        var template = "{{#for item in Items}}public string {{MissingProp}} { get; set; }{{/for}}";
        var context = new Dictionary<string, object> { ["Items"] = new List<object> { "Prop1", "Prop2" } };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<GenerationException>(() => service.RenderAsync(template, context));
        Assert.Contains("Error rendering template", exception.Message);
    }

    /// <summary>
    /// Test that unmatched if tags throw exception during validation.
    /// </summary>
    [Fact]
    public void ValidateTemplate_UnmatchedIfTags_ReturnsFalse()
    {
        // Arrange
        var service = CreateService();
        var template = "{{#if HasId}}public int Id { get; set; }";

        // Act
        var isValid = service.ValidateTemplate(template);

        // Assert
        Assert.False(isValid);
    }

    /// <summary>
    /// Test that unmatched for tags throw exception during validation.
    /// </summary>
    [Fact]
    public void ValidateTemplate_UnmatchedForTags_ReturnsFalse()
    {
        // Arrange
        var service = CreateService();
        var template = "{{#for prop in Properties}}public string {{prop}} { get; set; }";

        // Act
        var isValid = service.ValidateTemplate(template);

        // Assert
        Assert.False(isValid);
    }

    /// <summary>
    /// Test that null template throws ArgumentNullException.
    /// </summary>
    [Fact]
    public async Task RenderAsync_NullTemplate_ThrowsArgumentNullException()
    {
        // Arrange
        var service = CreateService();
        string template = null!;
        var context = CreateContext();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => service.RenderAsync(template, context));
    }

    /// <summary>
    /// Test that null context is handled gracefully (converted to empty dictionary).
    /// </summary>
    [Fact]
    public async Task RenderAsync_NullContext_HandledGracefully()
    {
        // Arrange
        var service = CreateService();
        var template = "public class {{Name}} { }";
        Dictionary<string, object> context = null!;

        // Act
        var result = await service.RenderAsync(template, context);

        // Assert - should not throw, context is converted to empty dict
        Assert.Equal("public class {{Name}} { }", result);
    }

    /// <summary>
    /// Test that multiple missing variables all throw exceptions.
    /// </summary>
    [Fact]
    public async Task RenderAsync_MultipleMissingVariables_ThrowsException()
    {
        // Arrange
        var service = CreateService();
        var template = "{{MissingVar1}} {{MissingVar2}} {{MissingVar3}}";
        var context = CreateContext();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<GenerationException>(() => service.RenderAsync(template, context));
        Assert.Contains("Error rendering template", exception.Message);
    }

    /// <summary>
    /// Test that missing variable in filter throws exception.
    /// </summary>
    [Fact]
    public async Task RenderAsync_MissingVariableInFilter_ThrowsClearException()
    {
        // Arrange
        var service = CreateService();
        var template = "{{MissingVar | upper}}";
        var context = CreateContext();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<GenerationException>(() => service.RenderAsync(template, context));
        Assert.Contains("Error rendering template", exception.Message);
    }

    /// <summary>
    /// Test that missing variable in special case transform throws exception.
    /// </summary>
    [Fact]
    public async Task RenderAsync_MissingVariableInSpecialCaseTransform_ThrowsClearException()
    {
        // Arrange
        var service = CreateService();
        var template = "public class {{snake_case}}";
        var context = CreateContext();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<GenerationException>(() => service.RenderAsync(template, context));
        Assert.Contains("Error rendering template", exception.Message);
    }
}