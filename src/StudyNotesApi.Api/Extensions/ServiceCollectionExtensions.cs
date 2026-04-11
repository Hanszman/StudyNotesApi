using StudyNotesApi.Api.HealthChecks;
using StudyNotesApi.Infrastructure.DependencyInjection;

namespace StudyNotesApi.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddInfrastructure(configuration);
        services.AddControllers();
        services.AddEndpointsApiExplorer();
        services.AddScoped<IDatabaseConnectionChecker, DatabaseConnectionChecker>();
        services
            .AddHealthChecks()
            .AddCheck("api", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy("API is online."))
            .AddCheck<DatabaseHealthCheck>("database");
        services.AddSwaggerGen();

        return services;
    }
}
