using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using StudyNotesApi.Api.DTOs.Common;

namespace StudyNotesApi.Api.Controllers;

[ApiController]
[Route("api/health")]
public class HealthController : ControllerBase
{
    private readonly IWebHostEnvironment _environment;
    private readonly HealthCheckService _healthCheckService;

    public HealthController(HealthCheckService healthCheckService, IWebHostEnvironment environment)
    {
        _healthCheckService = healthCheckService;
        _environment = environment;
    }

    [HttpGet]
    [ProducesResponseType<HealthResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<HealthResponse>(StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<HealthResponse>> Get(CancellationToken cancellationToken)
    {
        var report = await _healthCheckService.CheckHealthAsync(cancellationToken);
        var response = new HealthResponse(
            Status: report.Status.ToString(),
            Environment: _environment.EnvironmentName,
            ApiVersion: "v1",
            TimestampUtc: DateTime.UtcNow,
            Checks: report.Entries.ToDictionary(
                entry => entry.Key,
                entry => new HealthCheckEntryResponse(
                    Status: entry.Value.Status.ToString(),
                    Description: entry.Value.Description,
                    Duration: entry.Value.Duration.ToString())));

        return report.Status == HealthStatus.Healthy
            ? Ok(response)
            : StatusCode(StatusCodes.Status503ServiceUnavailable, response);
    }
}
