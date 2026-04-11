namespace StudyNotesApi.Api.HealthChecks;

public interface IDatabaseConnectionChecker
{
    Task<bool> CanConnectAsync(CancellationToken cancellationToken = default);
}
