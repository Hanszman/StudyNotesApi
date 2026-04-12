using FluentAssertions;
using Moq;
using StudyNotesApi.Application.Common.Exceptions;
using StudyNotesApi.Application.Interfaces.Repositories;
using StudyNotesApi.Application.Models.Common;
using StudyNotesApi.Application.Models.Tags;
using StudyNotesApi.Application.Services;
using StudyNotesApi.Domain.Entities;

namespace StudyNotesApi.UnitTests.Application.Services;

public class TagServiceTests
{
    [Fact]
    public async Task GetTagsAsync_should_delegate_to_repository()
    {
        var repository = new Mock<ITagRepository>();
        var service = new TagService(repository.Object);
        var userId = Guid.NewGuid();
        var expected = new PagedResult<Tag>([], 1, 10, 0);

        repository.Setup(value => value.GetByUserAsync(
                userId,
                It.IsAny<TagFilter>(),
                It.IsAny<PagedQuery>(),
                It.IsAny<SortRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await service.GetTagsAsync(
            userId,
            new TagFilter("dotnet"),
            new PagedQuery(),
            new SortRequest("name", SortDirection.Asc));

        result.Should().BeSameAs(expected);
    }

    [Fact]
    public async Task GetByIdAsync_should_return_tag_when_it_exists()
    {
        var repository = new Mock<ITagRepository>();
        var service = new TagService(repository.Object);
        var userId = Guid.NewGuid();
        var tag = new Tag("dotnet", userId);

        repository.Setup(value => value.GetByIdAsync(tag.Id, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tag);

        var result = await service.GetByIdAsync(userId, tag.Id);

        result.Should().BeSameAs(tag);
    }

    [Fact]
    public async Task GetByIdAsync_should_throw_not_found_when_tag_does_not_exist()
    {
        var repository = new Mock<ITagRepository>();
        var service = new TagService(repository.Object);

        repository.Setup(value => value.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Tag?)null);

        var action = async () => await service.GetByIdAsync(Guid.NewGuid(), Guid.NewGuid());

        await action.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task CreateAsync_should_persist_a_new_tag_when_name_is_available()
    {
        var repository = new Mock<ITagRepository>();
        var service = new TagService(repository.Object);
        var userId = Guid.NewGuid();

        repository.Setup(value => value.ExistsByNameAsync(userId, "dotnet", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await service.CreateAsync(userId, "  dotnet  ");

        result.Name.Should().Be("dotnet");
        result.UserId.Should().Be(userId);
        repository.Verify(value => value.AddAsync(
            It.Is<Tag>(tag => tag.Name == "dotnet" && tag.UserId == userId),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_should_throw_conflict_when_name_already_exists()
    {
        var repository = new Mock<ITagRepository>();
        var service = new TagService(repository.Object);
        var userId = Guid.NewGuid();

        repository.Setup(value => value.ExistsByNameAsync(userId, "dotnet", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var action = async () => await service.CreateAsync(userId, "dotnet");

        await action.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task CreateAsync_should_throw_validation_exception_when_name_is_missing()
    {
        var service = new TagService(Mock.Of<ITagRepository>());

        var action = async () => await service.CreateAsync(Guid.NewGuid(), "  ");

        await action.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task UpdateAsync_should_update_tag_when_name_is_available()
    {
        var repository = new Mock<ITagRepository>();
        var service = new TagService(repository.Object);
        var userId = Guid.NewGuid();
        var tag = new Tag("dotnet", userId);

        repository.Setup(value => value.GetByIdAsync(tag.Id, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tag);
        repository.Setup(value => value.ExistsByNameAsync(userId, "csharp", tag.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await service.UpdateAsync(userId, tag.Id, "  csharp  ");

        result.Name.Should().Be("csharp");
        result.UpdatedAt.Should().NotBeNull();
        repository.Verify(value => value.UpdateAsync(tag, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_should_throw_conflict_when_name_already_exists_for_another_tag()
    {
        var repository = new Mock<ITagRepository>();
        var service = new TagService(repository.Object);
        var userId = Guid.NewGuid();
        var tag = new Tag("dotnet", userId);

        repository.Setup(value => value.GetByIdAsync(tag.Id, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tag);
        repository.Setup(value => value.ExistsByNameAsync(userId, "csharp", tag.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var action = async () => await service.UpdateAsync(userId, tag.Id, "csharp");

        await action.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task UpdateAsync_should_throw_validation_exception_when_name_is_missing()
    {
        var repository = new Mock<ITagRepository>();
        var service = new TagService(repository.Object);
        var tag = new Tag("dotnet", Guid.NewGuid());

        repository.Setup(value => value.GetByIdAsync(tag.Id, tag.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tag);

        var action = async () => await service.UpdateAsync(tag.UserId, tag.Id, "");

        await action.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task DeleteAsync_should_remove_tag_when_it_exists()
    {
        var repository = new Mock<ITagRepository>();
        var service = new TagService(repository.Object);
        var tag = new Tag("dotnet", Guid.NewGuid());

        repository.Setup(value => value.GetByIdAsync(tag.Id, tag.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tag);

        await service.DeleteAsync(tag.UserId, tag.Id);

        repository.Verify(value => value.RemoveAsync(tag, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_should_throw_not_found_when_tag_does_not_exist()
    {
        var repository = new Mock<ITagRepository>();
        var service = new TagService(repository.Object);

        repository.Setup(value => value.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Tag?)null);

        var action = async () => await service.DeleteAsync(Guid.NewGuid(), Guid.NewGuid());

        await action.Should().ThrowAsync<NotFoundException>();
    }
}
