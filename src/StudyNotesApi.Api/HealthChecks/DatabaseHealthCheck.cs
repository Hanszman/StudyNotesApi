using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace StudyNotesApi.Api.HealthChecks;

public class DatabaseHealthCheck : IHealthCheck
{
    private readonly IDatabaseConnectionChecker _databaseConnectionChecker;

    public DatabaseHealthCheck(IDatabaseConnectionChecker databaseConnectionChecker)
    {
        _databaseConnectionChecker = databaseConnectionChecker;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var canConnect = await _databaseConnectionChecker.CanConnectAsync(cancellationToken);

        return canConnect
            ? HealthCheckResult.Healthy("Database connection is available.")
            : HealthCheckResult.Unhealthy("Database connection is unavailable.");
    }
}
