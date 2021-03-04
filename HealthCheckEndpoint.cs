#nullable enable
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace DotNetSourceGeneratorToolkit
{
    /// <summary>
    /// Represents a health check endpoint.
    /// </summary>
    public sealed class HealthCheckEndpoint
    {
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
        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            // Implement health check logic here
            return HealthCheckResult.Healthy();
        }
    }
}
