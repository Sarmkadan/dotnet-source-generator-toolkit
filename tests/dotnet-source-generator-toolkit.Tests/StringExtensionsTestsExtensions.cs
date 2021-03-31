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
        /// Returns the names of all public, parameter‑less test methods declared on <see cref="StringExtensionsTests"/>.
        /// </summary>
        /// <param name="test">The test instance.</param>
        /// <returns>An enumerable of method names.</returns>
        public static IEnumerable<string> GetTestMethodNames(this StringExtensionsTests test)
        {
            if (test == null) throw new ArgumentNullException(nameof(test));

            return test.GetType()
                .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
                .Where(m => m.GetParameters().Length == 0 && m.ReturnType == typeof(void))
                .Select(m => m.Name);
        }

        /// <summary>
        /// Executes every public, parameter‑less test method on the supplied <see cref="StringExtensionsTests"/> instance.
        /// </summary>
        /// <param name="test">The test instance.</param>
        /// <returns>
        /// A dictionary where the key is the test method name and the value is <c>null</c> if the test succeeded,
        /// or the exception that caused the failure.
        /// </returns>
        public static Dictionary<string, Exception?> RunAllTests(this StringExtensionsTests test)
        {
            if (test == null) throw new ArgumentNullException(nameof(test));

            var results = new Dictionary<string, Exception?>();

            var methods = test.GetType()
                .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
                .Where(m => m.GetParameters().Length == 0 && m.ReturnType == typeof(void));

            foreach (var method in methods)
            {
                try
                {
                    method.Invoke(test, null);
                    results[method.Name] = null; // success
                }
                catch (TargetInvocationException tie) when (tie.InnerException != null)
                {
                    results[method.Name] = tie.InnerException; // unwrap the actual test failure
                }
                catch (Exception ex)
                {
                    results[method.Name] = ex; // any other reflection‑related error
                }
            }

            return results;
        }
    }
}
