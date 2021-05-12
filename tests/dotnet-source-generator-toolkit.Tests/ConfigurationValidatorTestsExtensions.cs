using Xunit;
using DotNetSourceGeneratorToolkit.Configuration;

public static class ConfigurationValidatorTestsExtensions
{
    /// <summary>
    /// Verifies that the validator can successfully retrieve default configuration values.
    /// </summary>
    /// <param name="tests">The test instance.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="tests"/> is null.</exception>
    public static void VerifyDefaults(this ConfigurationValidatorTests tests)
    {
        ArgumentNullException.ThrowIfNull(tests);

        var validator = new ConfigurationValidator();
        var defaults = validator.GetDefaults();

        Assert.NotNull(defaults);
    }

    /// <summary>
    /// Verifies that validation fails for invalid options and returns appropriate error messages.
    /// </summary>
    /// <param name="tests">The test instance.</param>
    /// <param name="invalidOptions">The invalid options to validate.</param>
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
    /// Verifies that validation succeeds for valid options and returns no errors.
    /// </summary>
    /// <param name="tests">The test instance.</param>
    /// <param name="validOptions">The valid options to validate.</param>
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
