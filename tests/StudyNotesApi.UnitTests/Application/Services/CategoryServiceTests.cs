using FluentAssertions;
using Moq;
using StudyNotesApi.Application.Common.Exceptions;
using StudyNotesApi.Application.Interfaces.Repositories;
using StudyNotesApi.Application.Models.Categories;
using StudyNotesApi.Application.Models.Common;
using StudyNotesApi.Application.Services;
using StudyNotesApi.Domain.Entities;

namespace StudyNotesApi.UnitTests.Application.Services;

public class CategoryServiceTests
{
    [Fact]
    public async Task GetCategoriesAsync_should_delegate_to_repository()
    {
        var repository = new Mock<ICategoryRepository>();
        var service = new CategoryService(repository.Object);
        var userId = Guid.NewGuid();
        var expected = new PagedResult<Category>([], 1, 10, 0);

        repository.Setup(value => value.GetByUserAsync(
                userId,
                It.IsAny<CategoryFilter>(),
                It.IsAny<PagedQuery>(),
                It.IsAny<SortRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await service.GetCategoriesAsync(
            userId,
            new CategoryFilter("backend"),
            new PagedQuery(),
            new SortRequest("name", SortDirection.Asc));

        result.Should().BeSameAs(expected);
    }

    [Fact]
    public async Task GetByIdAsync_should_return_category_when_it_exists()
    {
        var repository = new Mock<ICategoryRepository>();
        var service = new CategoryService(repository.Object);
        var userId = Guid.NewGuid();
        var category = new Category("Backend", userId, "#2563EB");

        repository.Setup(value => value.GetByIdAsync(category.Id, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        var result = await service.GetByIdAsync(userId, category.Id);

        result.Should().BeSameAs(category);
    }

    [Fact]
    public async Task GetByIdAsync_should_throw_not_found_when_category_does_not_exist()
    {
        var repository = new Mock<ICategoryRepository>();
        var service = new CategoryService(repository.Object);

        repository.Setup(value => value.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Category?)null);

        var action = async () => await service.GetByIdAsync(Guid.NewGuid(), Guid.NewGuid());

        await action.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task CreateAsync_should_persist_a_new_category_when_name_is_available()
    {
        var repository = new Mock<ICategoryRepository>();
        var service = new CategoryService(repository.Object);
        var userId = Guid.NewGuid();

        repository.Setup(value => value.ExistsByNameAsync(userId, "Backend", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await service.CreateAsync(userId, "  Backend  ", " #2563EB ");

        result.Name.Should().Be("Backend");
        result.Color.Should().Be("#2563EB");
        result.UserId.Should().Be(userId);
        repository.Verify(value => value.AddAsync(
            It.Is<Category>(category =>
                category.Name == "Backend" &&
                category.Color == "#2563EB" &&
                category.UserId == userId),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_should_throw_conflict_when_name_already_exists()
    {
        var repository = new Mock<ICategoryRepository>();
        var service = new CategoryService(repository.Object);
        var userId = Guid.NewGuid();

        repository.Setup(value => value.ExistsByNameAsync(userId, "Backend", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var action = async () => await service.CreateAsync(userId, "Backend", null);

        await action.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task CreateAsync_should_throw_validation_exception_when_name_is_missing()
    {
        var service = new CategoryService(Mock.Of<ICategoryRepository>());

        var action = async () => await service.CreateAsync(Guid.NewGuid(), "   ", null);

        await action.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task UpdateAsync_should_update_category_when_name_is_available()
    {
        var repository = new Mock<ICategoryRepository>();
        var service = new CategoryService(repository.Object);
        var userId = Guid.NewGuid();
        var category = new Category("Old Name", userId, "#111111");

        repository.Setup(value => value.GetByIdAsync(category.Id, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);
        repository.Setup(value => value.ExistsByNameAsync(userId, "New Name", category.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await service.UpdateAsync(userId, category.Id, "  New Name  ", "  #222222  ");

        result.Name.Should().Be("New Name");
        result.Color.Should().Be("#222222");
        result.UpdatedAt.Should().NotBeNull();
        repository.Verify(value => value.UpdateAsync(category, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_should_throw_conflict_when_name_already_exists_for_another_category()
    {
        var repository = new Mock<ICategoryRepository>();
        var service = new CategoryService(repository.Object);
        var userId = Guid.NewGuid();
        var category = new Category("Old Name", userId);

        repository.Setup(value => value.GetByIdAsync(category.Id, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);
        repository.Setup(value => value.ExistsByNameAsync(userId, "Backend", category.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var action = async () => await service.UpdateAsync(userId, category.Id, "Backend", null);

        await action.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task UpdateAsync_should_throw_validation_exception_when_name_is_missing()
    {
        var repository = new Mock<ICategoryRepository>();
        var service = new CategoryService(repository.Object);
        var category = new Category("Backend", Guid.NewGuid());

        repository.Setup(value => value.GetByIdAsync(category.Id, category.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        var action = async () => await service.UpdateAsync(category.UserId, category.Id, "", null);

        await action.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task DeleteAsync_should_remove_category_when_it_exists()
    {
        var repository = new Mock<ICategoryRepository>();
        var service = new CategoryService(repository.Object);
        var category = new Category("Backend", Guid.NewGuid());

        repository.Setup(value => value.GetByIdAsync(category.Id, category.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        await service.DeleteAsync(category.UserId, category.Id);

        repository.Verify(value => value.RemoveAsync(category, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_should_throw_not_found_when_category_does_not_exist()
    {
        var repository = new Mock<ICategoryRepository>();
        var service = new CategoryService(repository.Object);

        repository.Setup(value => value.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Category?)null);

        var action = async () => await service.DeleteAsync(Guid.NewGuid(), Guid.NewGuid());

        await action.Should().ThrowAsync<NotFoundException>();
    }
}
