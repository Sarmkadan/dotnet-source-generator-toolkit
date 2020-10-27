#nullable enable
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace DotNetSourceGeneratorToolkit
{
    public sealed class HealthCheckEndpoint
    {
        public HealthCheckEndpoint()
        {
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            // Implement health check logic here
            return HealthCheckResult.Healthy();
        }
    }
}