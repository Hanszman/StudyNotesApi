using Microsoft.EntityFrameworkCore;
using StudyNotesApi.Infrastructure.Data.Context;

namespace StudyNotesApi.UnitTests.Infrastructure.Repositories;

internal static class RepositoryTestContextFactory
{
    public static ApplicationDbContext Create()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }
}
