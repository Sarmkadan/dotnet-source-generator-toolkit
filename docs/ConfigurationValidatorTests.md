# ConfigurationValidatorTests

The `ConfigurationValidatorTests` class comprises the comprehensive test suite for the `ConfigurationValidator` component within the `dotnet-source-generator-toolkit`. Its primary purpose is to verify the correctness of configuration validation logic, ensure that default configuration values are properly initialized, and validate interaction behavior when using mocked dependencies.

## API

*   **`Validate_WithNullOptions_ReturnsInvalidResultWithError`**
    Verifies that passing a null configuration object to the validator results in an invalid validation state and correctly identifies the error.

*   **`Validate_WithValidOptions_ReturnsValidResultWithNoErrors`**
    Ensures that providing a configuration object with valid property values results in a successful validation state without any reported errors.

*   **`Validate_WhenTimeoutBelowMinimum_AddsTimeoutError`**
    Tests that configuring a timeout value lower than the enforced minimum threshold correctly results in an invalid validation state with a specific timeout-related error message.

*   **`GetDefaults_ReturnsOptionsWithExpectedValues`**
    Confirms that the `GetDefaults` method successfully produces a configuration instance initialized with the expected default values.

*   **`MockedValidator_WhenConfiguredToReturnFailure_VerifiesCallAndReturnsConfiguredResult`**
    Validates that interactions with a mocked validator are correctly verified and that the method returns a configured failure result when explicitly instructed to do so.

## Usage

```csharp
// Example: Validating invalid configuration
[Fact]
public void Validation_Failure_Example()
{
    var validator = new ConfigurationValidator();
    var options = new ConfigOptions { Timeout = 0 }; // Below minimum
    
    var result = validator.Validate(options);
    
    Assert.False(result.IsValid);
    Assert.Contains(result.Errors, e => e.Code == "InvalidTimeout");
}

// Example: Verifying default configuration values
[Fact]
public void Defaults_Verification_Example()
{
    var options = ConfigurationValidator.GetDefaults();
    
    Assert.Equal(30, options.Timeout);
    Assert.True(options.Enabled);
}
```

## Notes

*   **Edge Cases:** The suite covers boundary conditions including null input, timeout values at the absolute minimum threshold, and cases where multiple configuration properties are invalid simultaneously.
*   **Thread Safety:** These tests are intended for execution within a standard xUnit test runner. As individual test methods maintain isolated state, they are safe for parallel execution. Should the `ConfigurationValidator` implementation be updated to utilize shared static state, test isolation should be re-evaluated to prevent cross-test interference.
