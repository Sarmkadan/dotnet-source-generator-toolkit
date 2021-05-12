using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DotNetSourceGeneratorToolkit.Tests
{
    /// <summary>
    /// Extension methods that make it easier to work with <see cref="StringExtensionsTests"/> instances in test code.
    /// </summary>
    public static class StringExtensionsTestsExtensions
    {
        /// <summary>
        /// Returns the names of all public, parameterless test methods declared on <see cref="StringExtensionsTests"/>.
        /// </summary>
        /// <param name="test">The test instance.</param>
        /// <returns>An enumerable of method names.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="test"/> is null.</exception>
        public static IEnumerable<string> GetTestMethodNames(this StringExtensionsTests test)
        {
            ArgumentNullException.ThrowIfNull(test);

            return test.GetType()
                .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
                .Where(static m => m.GetParameters().Length == 0 && m.ReturnType == typeof(void))
                .Select(static m => m.Name);
        }

        /// <summary>
        /// Executes every public, parameterless test method on the supplied <see cref="StringExtensionsTests"/> instance.
        /// </summary>
        /// <param name="test">The test instance.</param>
        /// <returns>
        /// A dictionary where the key is the test method name and the value is <c>null</c> if the test succeeded,
        /// or the exception that caused the failure.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="test"/> is null.</exception>
        public static Dictionary<string, Exception?> RunAllTests(this StringExtensionsTests test)
        {
            ArgumentNullException.ThrowIfNull(test);

            var results = new Dictionary<string, Exception?>();

            var methods = test.GetType()
                .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
                .Where(static m => m.GetParameters().Length == 0 && m.ReturnType == typeof(void));

            foreach (var method in methods)
            {
                try
                {
                    method.Invoke(test, null);
                    results[method.Name] = null; // success
                }
                catch (TargetInvocationException tie) when (tie.InnerException is not null)
                {
                    results[method.Name] = tie.InnerException; // unwrap the actual test failure
                }
                catch (Exception ex) when (ex is not TargetInvocationException)
                {
                    results[method.Name] = ex; // any other reflection-related error
                }
            }

            return results;
        }
    }
}