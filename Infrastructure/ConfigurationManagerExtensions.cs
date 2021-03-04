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
        public static string GetValueOrDefault(this ConfigurationManager configuration, string key, string defaultValue = "")
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException("Key cannot be null or empty.", nameof(key));
            }

            return configuration.HasKey(key) ? configuration.GetValue(key) : defaultValue;
        }

        /// <summary>
        /// Gets a configuration value by key, throwing an exception if the key does not exist.
        /// </summary>
        /// <param name="configuration">The configuration manager instance.</param>
        /// <param name="key">The configuration key to retrieve.</param>
        /// <returns>The configuration value.</returns>
        /// <exception cref="KeyNotFoundException">Thrown when the key is not found.</exception>
        public static string GetRequiredValue(this ConfigurationManager configuration, string key)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException("Key cannot be null or empty.", nameof(key));
            }

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
        public static T GetValue<T>(this ConfigurationManager configuration, string key)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException("Key cannot be null or empty.", nameof(key));
            }

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
        public static T GetValueOrDefault<T>(this ConfigurationManager configuration, string key, T defaultValue = default)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException("Key cannot be null or empty.", nameof(key));
            }

            if (!configuration.HasKey(key))
            {
                return defaultValue;
            }

            try
            {
                var value = configuration.GetValue(key);
                return (T)Convert.ChangeType(value, typeof(T));
            }
            catch
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// Gets all configuration values as a dictionary.
        /// </summary>
        /// <param name="configuration">The configuration manager instance.</param>
        /// <returns>A read-only dictionary of all configuration values.</returns>
        public static IReadOnlyDictionary<string, string> GetAllConfig(this ConfigurationManager configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            return configuration.GetAllConfig();
        }
    }
}
