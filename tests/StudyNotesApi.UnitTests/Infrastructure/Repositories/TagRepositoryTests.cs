using FluentAssertions;
using StudyNotesApi.Application.Common.Exceptions;
using StudyNotesApi.Application.Models.Common;
using StudyNotesApi.Application.Models.Tags;
using StudyNotesApi.Domain.Entities;
using StudyNotesApi.Infrastructure.Repositories;

namespace StudyNotesApi.UnitTests.Infrastructure.Repositories;

public class TagRepositoryTests
{
    [Fact]
    public async Task AddAsync_and_RemoveAsync_should_persist_and_delete_a_tag()
    {
        await using var dbContext = RepositoryTestContextFactory.Create();
        var repository = new TagRepository(dbContext);
        var tag = new Tag("dotnet", Guid.NewGuid());

        await repository.AddAsync(tag);
        dbContext.Tags.Should().ContainSingle();

        await repository.RemoveAsync(tag);
        dbContext.Tags.Should().BeEmpty();
    }

    [Fact]
    public async Task ExistsByNameAsync_should_support_user_scope_and_exclusion()
    {
        await using var dbContext = RepositoryTestContextFactory.Create();
        var repository = new TagRepository(dbContext);
        var userId = Guid.NewGuid();
        var tag = new Tag("dotnet", userId);
        await repository.AddAsync(tag);

        (await repository.ExistsByNameAsync(userId, "dotnet")).Should().BeTrue();
        (await repository.ExistsByNameAsync(userId, "dotnet", tag.Id)).Should().BeFalse();
        (await repository.ExistsByNameAsync(Guid.NewGuid(), "dotnet")).Should().BeFalse();
    }

    [Fact]
    public async Task GetByIdAsync_and_GetByIdsAsync_should_only_return_requested_user_tags()
    {
        await using var dbContext = RepositoryTestContextFactory.Create();
        var repository = new TagRepository(dbContext);
        var userId = Guid.NewGuid();
        var tagA = new Tag("dotnet", userId);
        var tagB = new Tag("csharp", userId);
        var otherUserTag = new Tag("blocked", Guid.NewGuid());

        await repository.AddAsync(tagA);
        await repository.AddAsync(tagB);
        await repository.AddAsync(otherUserTag);

        var byId = await repository.GetByIdAsync(tagA.Id, userId);
        var byIds = await repository.GetByIdsAsync(userId, [tagA.Id, otherUserTag.Id]);

        byId.Should().NotBeNull();
        byIds.Should().ContainSingle(tag => tag.Id == tagA.Id);
    }

    [Fact]
    public async Task GetByUserAsync_should_filter_sort_paginate_and_apply_default_sorting()
    {
        await using var dbContext = RepositoryTestContextFactory.Create();
        var repository = new TagRepository(dbContext);
        var userId = Guid.NewGuid();

        var older = new Tag("backend", userId);
        var newer = new Tag("dotnet", userId);
        var ignored = new Tag("frontend", Guid.NewGuid());

        EntityPropertySetter.SetProperty(older, nameof(Tag.CreatedAt), new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc));
        EntityPropertySetter.SetProperty(newer, nameof(Tag.CreatedAt), new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc));

        await repository.AddAsync(older);
        await repository.AddAsync(newer);
        await repository.AddAsync(ignored);

        var filtered = await repository.GetByUserAsync(
            userId,
            new TagFilter("  net  "),
            new PagedQuery(1, 10),
            new SortRequest("name", SortDirection.Asc));

        var defaultSorted = await repository.GetByUserAsync(
            userId,
            new TagFilter(),
            new PagedQuery(1, 1),
            new SortRequest());

        filtered.TotalCount.Should().Be(1);
        filtered.Items.Select(tag => tag.Name).Should().ContainSingle().Which.Should().Be("dotnet");
        defaultSorted.Items.Select(tag => tag.Name).Should().ContainSingle().Which.Should().Be("dotnet");
        defaultSorted.HasNextPage.Should().BeTrue();
    }

    [Fact]
    public async Task GetByUserAsync_should_throw_for_invalid_sort_field()
    {
        await using var dbContext = RepositoryTestContextFactory.Create();
        var repository = new TagRepository(dbContext);

        var action = async () => await repository.GetByUserAsync(
            Guid.NewGuid(),
            new TagFilter(),
            new PagedQuery(),
            new SortRequest("invalidField"));

        await action.Should().ThrowAsync<ValidationException>();
    }
}
