using StudyNotesApi.Infrastructure.Data.Context;

namespace StudyNotesApi.Api.HealthChecks;

public class DatabaseConnectionChecker : IDatabaseConnectionChecker
{
    private readonly ApplicationDbContext _dbContext;

    public DatabaseConnectionChecker(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<bool> CanConnectAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.Database.CanConnectAsync(cancellationToken);
    }
}
