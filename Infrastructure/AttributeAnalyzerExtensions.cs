#nullable enable
using System.Collections.Generic;
using System.Linq;

namespace DotNetSourceGeneratorToolkit.Infrastructure
{
    /// <summary>
    /// Provides extension methods for <see cref="AttributeAnalyzer"/>.
    /// </summary>
    public static class AttributeAnalyzerExtensions
    {
        /// <summary>
        /// Determines whether any attribute in the source code has the specified name and parameter value.
        /// </summary>
        /// <param name="analyzer">The attribute analyzer.</param>
        /// <param name="sourceCode">The source code to analyze.</param>
        /// <param name="attributeName">Name of the attribute.</param>
        /// <param name="parameterKey">The parameter key to check.</param>
        /// <param name="expectedValue">The expected parameter value.</param>
        /// <returns><c>true</c> if the attribute exists with the specified parameter value; otherwise, <c>false</c>.</returns>
        public static bool HasAttributeWithParameter(
            this AttributeAnalyzer analyzer,
            string sourceCode,
            string attributeName,
            string parameterKey,
            string expectedValue)
        {
            if (!analyzer.HasAttribute(sourceCode, attributeName))
            {
                return false;
            }

            var parameters = analyzer.GetAttributeParameters(sourceCode, attributeName);
            return parameters?.ContainsKey(parameterKey) == true && parameters[parameterKey] == expectedValue;
        }

        /// <summary>
        /// Determines whether any attribute satisfies the specified condition.
        /// </summary>
        /// <param name="analyzer">The attribute analyzer.</param>
        /// <param name="sourceCode">The source code to analyze.</param>
        /// <param name="predicate">The condition to satisfy.</param>
        /// <returns><c>true</c> if any attribute satisfies the condition; otherwise, <c>false</c>.</returns>
        public static bool AnyAttributeSatisfies(
            this AttributeAnalyzer analyzer,
            string sourceCode,
            System.Func<AttributeInfo, bool> predicate)
        {
            return analyzer.AnalyzeAttributes(sourceCode).Any(predicate);
        }

        /// <summary>
        /// Counts the number of attributes in the source code.
        /// </summary>
        /// <param name="analyzer">The attribute analyzer.</param>
        /// <param name="sourceCode">The source code to analyze.</param>
        /// <returns>The number of attributes.</returns>
        public static int CountAttributes(
            this AttributeAnalyzer analyzer,
            string sourceCode)
        {
            return analyzer.AnalyzeAttributes(sourceCode).Count();
        }
    }
}
