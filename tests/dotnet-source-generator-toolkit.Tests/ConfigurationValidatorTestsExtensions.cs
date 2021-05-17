using Xunit;
using DotNetSourceGeneratorToolkit.Configuration;

/// <summary>
/// Provides extension methods for <see cref="ConfigurationValidatorTests"/> to validate configuration validation scenarios.
/// </summary>
public static class ConfigurationValidatorTestsExtensions
{
    /// <summary>
    /// Verifies that the <see cref="ConfigurationValidator.GetDefaults"/> method returns non-null default configuration values.
    /// </summary>
    /// <param name="tests">The test instance providing context for validation.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="tests"/> is null.</exception>
    public static void VerifyDefaults(this ConfigurationValidatorTests tests)
    {
        ArgumentNullException.ThrowIfNull(tests);

        var validator = new ConfigurationValidator();
        var defaults = validator.GetDefaults();

        Assert.NotNull(defaults);
    }

    /// <summary>
    /// Verifies that validation of invalid configuration options fails and produces error messages.
    /// </summary>
    /// <param name="tests">The test instance providing context for validation.</param>
    /// <param name="invalidOptions">The invalid configuration options to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="tests"/> or <paramref name="invalidOptions"/> is null.</exception>
    public static void VerifyValidationFailure(this ConfigurationValidatorTests tests, ToolkitOptions invalidOptions)
    {
        ArgumentNullException.ThrowIfNull(tests);
        ArgumentNullException.ThrowIfNull(invalidOptions);

        var validator = new ConfigurationValidator();
        var result = validator.Validate(invalidOptions);

        Assert.False(result.IsValid);
        Assert.NotEmpty(result.Errors);
    }

    /// <summary>
    /// Verifies that validation of valid configuration options succeeds and produces no errors.
    /// </summary>
    /// <param name="tests">The test instance providing context for validation.</param>
    /// <param name="validOptions">The valid configuration options to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="tests"/> or <paramref name="validOptions"/> is null.</exception>
    public static void VerifyValidationSuccess(this ConfigurationValidatorTests tests, ToolkitOptions validOptions)
    {
        ArgumentNullException.ThrowIfNull(tests);
        ArgumentNullException.ThrowIfNull(validOptions);

        var validator = new ConfigurationValidator();
        var result = validator.Validate(validOptions);

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }
}
