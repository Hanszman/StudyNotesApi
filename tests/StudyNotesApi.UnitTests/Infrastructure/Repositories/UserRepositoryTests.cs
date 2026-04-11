using FluentAssertions;
using StudyNotesApi.Domain.Entities;
using StudyNotesApi.Infrastructure.Repositories;

namespace StudyNotesApi.UnitTests.Infrastructure.Repositories;

public class UserRepositoryTests
{
    [Fact]
    public async Task AddAsync_should_persist_a_user()
    {
        await using var dbContext = RepositoryTestContextFactory.Create();
        var repository = new UserRepository(dbContext);
        var user = new User("Victor", "victor@email.com", "hash");

        await repository.AddAsync(user);

        dbContext.Users.Should().ContainSingle(savedUser => savedUser.Id == user.Id);
    }

    [Fact]
    public async Task EmailExistsAsync_should_return_expected_result()
    {
        await using var dbContext = RepositoryTestContextFactory.Create();
        var repository = new UserRepository(dbContext);
        var user = new User("Victor", "victor@email.com", "hash");
        await repository.AddAsync(user);

        (await repository.EmailExistsAsync("victor@email.com")).Should().BeTrue();
        (await repository.EmailExistsAsync("other@email.com")).Should().BeFalse();
    }

    [Fact]
    public async Task GetByEmailAsync_and_GetByIdAsync_should_return_the_expected_user()
    {
        await using var dbContext = RepositoryTestContextFactory.Create();
        var repository = new UserRepository(dbContext);
        var user = new User("Victor", "victor@email.com", "hash");
        await repository.AddAsync(user);

        var byEmail = await repository.GetByEmailAsync("victor@email.com");
        var byId = await repository.GetByIdAsync(user.Id);

        byEmail.Should().NotBeNull();
        byId.Should().NotBeNull();
        byEmail!.Id.Should().Be(user.Id);
        byId!.Email.Should().Be("victor@email.com");
    }
}
