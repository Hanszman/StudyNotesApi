using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Moq;
using StudyNotesApi.Api.Controllers;
using StudyNotesApi.Api.DTOs.Common;

namespace StudyNotesApi.UnitTests.Api.Controllers;

public class HealthControllerTests
{
    [Fact]
    public async Task Get_should_return_ok_when_health_report_is_healthy()
    {
        var report = new HealthReport(new Dictionary<string, HealthReportEntry>
        {
            ["api"] = new(HealthStatus.Healthy, "API is online.", TimeSpan.FromMilliseconds(1), null, null),
            ["database"] = new(HealthStatus.Healthy, "Database connection is available.", TimeSpan.FromMilliseconds(2), null, null)
        }, TimeSpan.FromMilliseconds(3));

        var healthCheckService = new FakeHealthCheckService(report);
        var environment = new Mock<IWebHostEnvironment>();
        environment.SetupGet(value => value.EnvironmentName).Returns(Environments.Development);
        var controller = new HealthController(healthCheckService, environment.Object);

        var result = await controller.Get(CancellationToken.None);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<HealthResponse>().Subject;

        response.Status.Should().Be("Healthy");
        response.Checks.Should().ContainKey("api");
        response.Checks.Should().ContainKey("database");
    }

    [Fact]
    public async Task Get_should_return_service_unavailable_when_health_report_is_unhealthy()
    {
        var report = new HealthReport(new Dictionary<string, HealthReportEntry>
        {
            ["database"] = new(HealthStatus.Unhealthy, "Database connection is unavailable.", TimeSpan.FromMilliseconds(2), null, null)
        }, TimeSpan.FromMilliseconds(2));

        var healthCheckService = new FakeHealthCheckService(report);
        var environment = new Mock<IWebHostEnvironment>();
        environment.SetupGet(value => value.EnvironmentName).Returns(Environments.Development);
        var controller = new HealthController(healthCheckService, environment.Object);

        var result = await controller.Get(CancellationToken.None);

        var objectResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(503);
    }

    private sealed class FakeHealthCheckService : HealthCheckService
    {
        private readonly HealthReport _report;

        public FakeHealthCheckService(HealthReport report)
        {
            _report = report;
        }

        public override Task<HealthReport> CheckHealthAsync(
            Func<HealthCheckRegistration, bool>? predicate,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_report);
        }
    }
}
