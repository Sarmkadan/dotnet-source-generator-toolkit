#nullable enable

using System.Globalization;

namespace DotNetSourceGeneratorToolkit.Examples.V2BasicUsage
{
    /// <summary>
    /// Provides validation helpers for the <see cref="Product"/> class.
    /// </summary>
    public static class ProductValidation
    {
        /// <summary>
        /// Validates a <see cref="Product"/> instance and returns a list of human-readable validation problems.
        /// </summary>
        /// <param name="value">The product to validate.</param>
        /// <returns>A read-only list of validation problems, or empty if the product is valid.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
        public static IReadOnlyList<string> Validate(this Product value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var problems = new List<string>();

            // Validate Id
            if (value.Id <= 0)
            {
                problems.Add("Product Id must be a positive integer.");
            }

            // Validate Name
            if (string.IsNullOrWhiteSpace(value.Name))
            {
                problems.Add("Product Name cannot be null, empty, or whitespace.");
            }
            else if (value.Name.Length > 100)
            {
                problems.Add("Product Name cannot exceed 100 characters.");
            }

            // Validate Price
            if (value.Price < 0)
            {
                problems.Add("Product Price cannot be negative.");
            }

            // Validate CreatedDate
            if (value.CreatedDate == default)
            {
                problems.Add("Product CreatedDate cannot be the default DateTime value.");
            }
            else if (value.CreatedDate > DateTime.UtcNow.AddMinutes(5))
            {
                problems.Add("Product CreatedDate cannot be in the future.");
            }

            // Validate IsAvailable (no validation needed for boolean)

            return problems.AsReadOnly();
        }

        /// <summary>
        /// Determines whether a <see cref="Product"/> instance is valid.
        /// </summary>
        /// <param name="value">The product to check.</param>
        /// <returns>True if the product is valid; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
        public static bool IsValid(this Product value)
        {
            return value.Validate().Count == 0;
        }

        /// <summary>
        /// Ensures that a <see cref="Product"/> instance is valid, throwing an <see cref="ArgumentException"/>
        /// with a detailed message listing all validation problems if it is not.
        /// </summary>
        /// <param name="value">The product to validate.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when the product has validation problems.</exception>
        public static void EnsureValid(this Product value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var problems = value.Validate();

            if (problems.Count > 0)
            {
                throw new ArgumentException(
                    $"Product validation failed:{Environment.NewLine}{string.Join(Environment.NewLine, problems)}");
            }
        }
    }
}