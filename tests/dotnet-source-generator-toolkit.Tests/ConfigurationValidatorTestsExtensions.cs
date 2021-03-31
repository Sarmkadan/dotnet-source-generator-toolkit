using Xunit;
using DotNetSourceGeneratorToolkit.Configuration;

public static class ConfigurationValidatorTestsExtensions
{
    public static void VerifyDefaults(this ConfigurationValidatorTests tests)
    {
        // Arrange
        var validator = new ConfigurationValidator();

        // Act
        var defaults = validator.GetDefaults();

        // Assert
        Assert.NotNull(defaults);
    }

    public static void VerifyValidationFailure(this ConfigurationValidatorTests tests, ToolkitOptions invalidOptions)
    {
        // Arrange
        var validator = new ConfigurationValidator();

        // Act
        var result = validator.Validate(invalidOptions);

        // Assert
        Assert.False(result.IsValid);
        Assert.NotEmpty(result.Errors);
    }

    public static void VerifyValidationSuccess(this ConfigurationValidatorTests tests, ToolkitOptions validOptions)
    {
        // Arrange
        var validator = new ConfigurationValidator();

        // Act
        var result = validator.Validate(validOptions);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }
}
