#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace DotNetSourceGeneratorToolkit
{
    /// <summary>
    /// Represents a health check endpoint.
    /// </summary>
    public sealed class HealthCheckEndpoint
    {
        // Stopwatch to track uptime since the process started.
        private static readonly Stopwatch _stopwatch = Stopwatch.StartNew();

        /// <summary>
        /// Initializes a new instance of the <see cref="HealthCheckEndpoint"/> class.
        /// </summary>
        public HealthCheckEndpoint()
        {
        }

        /// <summary>
        /// Checks the health of the system.
        /// </summary>
        /// <param name="context">The health check context.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A health check result.</returns>
        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            // Gather health data
            var data = new Dictionary<string, object>
            {
                ["assemblyVersion"] = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "unknown",
                ["uptime"] = _stopwatch.Elapsed.ToString(),
                ["timestamp"] = DateTime.UtcNow
            };

            // Return a healthy result with the data payload
            return Task.FromResult(HealthCheckResult.Healthy("Healthy", data));
        }
    }
}
