using System;
using System.Collections.Generic;

namespace DotNetSourceGeneratorToolkit.Infrastructure
{
    public static class ConfigurationManagerExtensions
    {
        /// <summary>
        /// Gets a configuration value by key, returning a default value if the key does not exist.
        /// </summary>
        /// <param name="configuration">The configuration manager instance.</param>
        /// <param name="key">The configuration key to retrieve.</param>
        /// <param name="defaultValue">The default value to return if the key is not found.</param>
        /// <returns>The configuration value or the default value.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="configuration"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="key"/> is null or empty.</exception>
        public static string GetValueOrDefault(this ConfigurationManager configuration, string key, string defaultValue = "")
        {
            ArgumentNullException.ThrowIfNull(configuration);

            ArgumentException.ThrowIfNullOrEmpty(key);

            return configuration.HasKey(key) ? configuration.GetValue(key) : defaultValue;
        }

        /// <summary>
        /// Gets a configuration value by key, throwing an exception if the key does not exist.
        /// </summary>
        /// <param name="configuration">The configuration manager instance.</param>
        /// <param name="key">The configuration key to retrieve.</param>
        /// <returns>The configuration value.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="configuration"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="key"/> is null or empty.</exception>
        /// <exception cref="KeyNotFoundException">Thrown when the key is not found.</exception>
        public static string GetRequiredValue(this ConfigurationManager configuration, string key)
        {
            ArgumentNullException.ThrowIfNull(configuration);

            ArgumentException.ThrowIfNullOrEmpty(key);

            if (!configuration.HasKey(key))
            {
                throw new KeyNotFoundException($"The required configuration key '{key}' was not found.");
            }

            return configuration.GetValue(key);
        }

        /// <summary>
        /// Gets a configuration value by key, converting it to the specified type.
        /// </summary>
        /// <typeparam name="T">The type to convert the value to.</typeparam>
        /// <param name="configuration">The configuration manager instance.</param>
        /// <param name="key">The configuration key to retrieve.</param>
        /// <returns>The converted configuration value.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="configuration"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="key"/> is null or empty.</exception>
        /// <exception cref="InvalidCastException">Thrown when the value cannot be converted to type <typeparamref name="T"/>.</exception>
        /// <exception cref="FormatException">Thrown when the value format is invalid for the target type.</exception>
        /// <exception cref="OverflowException">Thrown when the value represents a number too large for the target type.</exception>
        public static T GetValue<T>(this ConfigurationManager configuration, string key)
        {
            ArgumentNullException.ThrowIfNull(configuration);

            ArgumentException.ThrowIfNullOrEmpty(key);

            var value = configuration.GetValue(key);
            return (T)Convert.ChangeType(value, typeof(T));
        }

        /// <summary>
        /// Gets a configuration value by key, returning a default value if the key does not exist or conversion fails.
        /// </summary>
        /// <typeparam name="T">The type to convert the value to.</typeparam>
        /// <param name="configuration">The configuration manager instance.</param>
        /// <param name="key">The configuration key to retrieve.</param>
        /// <param name="defaultValue">The default value to return if the key is not found or conversion fails.</param>
        /// <returns>The converted configuration value or the default value.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="configuration"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="key"/> is null or empty.</exception>
        public static T GetValueOrDefault<T>(this ConfigurationManager configuration, string key, T defaultValue = default)
        {
            ArgumentNullException.ThrowIfNull(configuration);

            ArgumentException.ThrowIfNullOrEmpty(key);

            if (!configuration.HasKey(key))
            {
                return defaultValue;
            }

            try
            {
                var value = configuration.GetValue(key);
                return (T)Convert.ChangeType(value, typeof(T));
            }
            catch (FormatException)
            {
                return defaultValue;
            }
            catch (OverflowException)
            {
                return defaultValue;
            }
            catch (InvalidCastException)
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// Gets all configuration values as a dictionary.
        /// </summary>
        /// <param name="configuration">The configuration manager instance.</param>
        /// <returns>A read-only dictionary of all configuration values.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="configuration"/> is null.</exception>
        public static IReadOnlyDictionary<string, string> GetAllConfig(this ConfigurationManager configuration)
        {
            ArgumentNullException.ThrowIfNull(configuration);

            return configuration.GetAllConfig();
        }
    }

}