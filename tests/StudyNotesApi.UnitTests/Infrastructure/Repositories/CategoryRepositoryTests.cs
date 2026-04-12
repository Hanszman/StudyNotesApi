using FluentAssertions;
using StudyNotesApi.Application.Common.Exceptions;
using StudyNotesApi.Application.Models.Categories;
using StudyNotesApi.Application.Models.Common;
using StudyNotesApi.Domain.Entities;
using StudyNotesApi.Infrastructure.Repositories;

namespace StudyNotesApi.UnitTests.Infrastructure.Repositories;

public class CategoryRepositoryTests
{
    [Fact]
    public async Task AddAsync_and_RemoveAsync_should_persist_and_delete_a_category()
    {
        await using var dbContext = RepositoryTestContextFactory.Create();
        var repository = new CategoryRepository(dbContext);
        var category = new Category("Backend", Guid.NewGuid(), "#111111");

        await repository.AddAsync(category);
        dbContext.Categories.Should().ContainSingle();

        await repository.RemoveAsync(category);
        dbContext.Categories.Should().BeEmpty();
    }

    [Fact]
    public async Task UpdateAsync_should_persist_changes_to_a_category()
    {
        await using var dbContext = RepositoryTestContextFactory.Create();
        var repository = new CategoryRepository(dbContext);
        var category = new Category("Backend", Guid.NewGuid(), "#111111");
        await repository.AddAsync(category);

        category.Rename("Architecture");
        category.UpdateColor("#222222");

        await repository.UpdateAsync(category);

        var updated = await repository.GetByIdAsync(category.Id, category.UserId);
        updated.Should().NotBeNull();
        updated!.Name.Should().Be("Architecture");
        updated.Color.Should().Be("#222222");
    }

    [Fact]
    public async Task ExistsByNameAsync_should_support_user_scope_and_exclusion()
    {
        await using var dbContext = RepositoryTestContextFactory.Create();
        var repository = new CategoryRepository(dbContext);
        var userId = Guid.NewGuid();
        var category = new Category("Backend", userId);
        await repository.AddAsync(category);

        (await repository.ExistsByNameAsync(userId, "Backend")).Should().BeTrue();
        (await repository.ExistsByNameAsync(userId, "Backend", category.Id)).Should().BeFalse();
        (await repository.ExistsByNameAsync(Guid.NewGuid(), "Backend")).Should().BeFalse();
    }

    [Fact]
    public async Task GetByIdAsync_should_only_return_categories_from_the_requested_user()
    {
        await using var dbContext = RepositoryTestContextFactory.Create();
        var repository = new CategoryRepository(dbContext);
        var userId = Guid.NewGuid();
        var category = new Category("Backend", userId);
        await repository.AddAsync(category);

        var found = await repository.GetByIdAsync(category.Id, userId);
        var notFound = await repository.GetByIdAsync(category.Id, Guid.NewGuid());

        found.Should().NotBeNull();
        notFound.Should().BeNull();
    }

    [Fact]
    public async Task GetByUserAsync_should_filter_sort_paginate_and_apply_default_sorting()
    {
        await using var dbContext = RepositoryTestContextFactory.Create();
        var repository = new CategoryRepository(dbContext);
        var userId = Guid.NewGuid();

        var architecture = new Category("Architecture", userId, "#222222");
        var backend = new Category("Backend", userId, "#111111");
        var ignored = new Category("Frontend", Guid.NewGuid(), "#333333");

        EntityPropertySetter.SetProperty(architecture, nameof(Category.CreatedAt), new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc));
        EntityPropertySetter.SetProperty(backend, nameof(Category.CreatedAt), new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc));

        await repository.AddAsync(architecture);
        await repository.AddAsync(backend);
        await repository.AddAsync(ignored);

        var filtered = await repository.GetByUserAsync(
            userId,
            new CategoryFilter("  end  "),
            new PagedQuery(1, 10),
            new SortRequest("name", SortDirection.Asc));

        var defaultSorted = await repository.GetByUserAsync(
            userId,
            new CategoryFilter(),
            new PagedQuery(1, 1),
            new SortRequest());

        filtered.TotalCount.Should().Be(1);
        filtered.Items.Select(category => category.Name).Should().ContainSingle().Which.Should().Be("Backend");
        defaultSorted.Items.Select(category => category.Name).Should().ContainSingle().Which.Should().Be("Backend");
        defaultSorted.HasNextPage.Should().BeTrue();
    }

    [Fact]
    public async Task GetByUserAsync_should_throw_for_invalid_sort_field()
    {
        await using var dbContext = RepositoryTestContextFactory.Create();
        var repository = new CategoryRepository(dbContext);

        var action = async () => await repository.GetByUserAsync(
            Guid.NewGuid(),
            new CategoryFilter(),
            new PagedQuery(),
            new SortRequest("invalidField"));

        await action.Should().ThrowAsync<ValidationException>();
    }
}
