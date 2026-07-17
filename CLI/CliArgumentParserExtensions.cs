using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DotNetSourceGeneratorToolkit.CLI
{
	/// <summary>
	/// Provides useful extension methods for <see cref="CliArgumentParser"/>.
	/// </summary>
	public static class CliArgumentParserExtensions
	{
		/// <summary>
		/// Returns a single string that contains both the help message and the version information.
		/// </summary>
		/// <param name="parser">The <see cref="CliArgumentParser"/> instance.</param>
		/// <returns>The help message, a line break, and then the version information.</returns>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="parser"/> is <c>null</c>.</exception>
		public static string GetFullHelp(this CliArgumentParser parser)
		{
			ArgumentNullException.ThrowIfNull(parser);
			return string.Concat(parser.GetHelpMessage(), Environment.NewLine, parser.GetVersionInfo());
		}

		/// <summary>
		/// Validates the supplied <see cref="CliOptions"/> and returns the validation messages as a read-only list.
		/// </summary>
		/// <param name="parser">The <see cref="CliArgumentParser"/> instance.</param>
		/// <param name="options">The options to validate.</param>
		/// <returns>A read-only list of validation messages. The list is empty when the options are valid.</returns>
		/// <exception cref="ArgumentNullException">
		/// Thrown when <paramref name="parser"/> or <paramref name="options"/> is <c>null</c>.
		/// </exception>
		public static IReadOnlyList<string> ValidateOptions(this CliArgumentParser parser, CliOptions options)
		{
			ArgumentNullException.ThrowIfNull(parser);
			ArgumentNullException.ThrowIfNull(options);
			return parser.Validate(options).ToList().AsReadOnly();
		}

		/// <summary>
		/// Attempts to validate the supplied <see cref="CliOptions"/>.
		/// </summary>
		/// <param name="parser">The <see cref="CliArgumentParser"/> instance.</param>
		/// <param name="options">The options to validate.</param>
		/// <param name="errors">
		/// When the method returns, contains a read-only list of validation messages if the validation fails;
		/// otherwise an empty list.
		/// </param>
		/// <returns><c>true</c> if the options are valid; otherwise <c>false</c>.</returns>
		/// <exception cref="ArgumentNullException">
		/// Thrown when <paramref name="parser"/> or <paramref name="options"/> is <c>null</c>.
		/// </exception>
		public static bool TryValidate(this CliArgumentParser parser, CliOptions options, out IReadOnlyList<string> errors)
		{
			ArgumentNullException.ThrowIfNull(parser);
			ArgumentNullException.ThrowIfNull(options);
			var list = parser.Validate(options).ToList();
			errors = list.AsReadOnly();
			return list.Count == 0;
		}

		/// <summary>
		/// Writes the help and version information to the specified <see cref="TextWriter"/>.
		/// </summary>
		/// <param name="parser">The <see cref="CliArgumentParser"/> instance.</param>
		/// <param name="writer">
		/// The writer to which the messages are written. If <c>null</c>, <see cref="Console.Out"/> is used.
		/// </param>
		/// <exception cref="ArgumentNullException">
		/// Thrown when <paramref name="parser"/> is <c>null</c>.
		/// </exception>
		public static void PrintHelp(this CliArgumentParser parser, TextWriter? writer = null)
		{
			ArgumentNullException.ThrowIfNull(parser);
			writer ??= Console.Out;
			writer.WriteLine(parser.GetHelpMessage());
			writer.WriteLine(parser.GetVersionInfo());
		}
	}
}
