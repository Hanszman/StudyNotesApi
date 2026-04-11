using FluentAssertions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Moq;
using StudyNotesApi.Api.HealthChecks;

namespace StudyNotesApi.UnitTests.Api.HealthChecks;

public class DatabaseHealthCheckTests
{
    [Fact]
    public async Task CheckHealthAsync_should_return_healthy_when_database_connection_is_available()
    {
        var checker = new Mock<IDatabaseConnectionChecker>();
        checker.Setup(value => value.CanConnectAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);
        var healthCheck = new DatabaseHealthCheck(checker.Object);

        var result = await healthCheck.CheckHealthAsync(new HealthCheckContext(), CancellationToken.None);

        result.Status.Should().Be(HealthStatus.Healthy);
        result.Description.Should().Be("Database connection is available.");
    }

    [Fact]
    public async Task CheckHealthAsync_should_return_unhealthy_when_database_connection_is_unavailable()
    {
        var checker = new Mock<IDatabaseConnectionChecker>();
        checker.Setup(value => value.CanConnectAsync(It.IsAny<CancellationToken>())).ReturnsAsync(false);
        var healthCheck = new DatabaseHealthCheck(checker.Object);

        var result = await healthCheck.CheckHealthAsync(new HealthCheckContext(), CancellationToken.None);

        result.Status.Should().Be(HealthStatus.Unhealthy);
        result.Description.Should().Be("Database connection is unavailable.");
    }
}
