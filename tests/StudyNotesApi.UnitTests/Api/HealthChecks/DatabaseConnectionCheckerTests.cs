using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using StudyNotesApi.Api.HealthChecks;
using StudyNotesApi.Infrastructure.Data.Context;

namespace StudyNotesApi.UnitTests.Api.HealthChecks;

public class DatabaseConnectionCheckerTests
{
    [Fact]
    public async Task CanConnectAsync_should_return_true_for_an_available_database_provider()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var dbContext = new ApplicationDbContext(options);
        var checker = new DatabaseConnectionChecker(dbContext);

        var result = await checker.CanConnectAsync(CancellationToken.None);

        result.Should().BeTrue();
    }
}
