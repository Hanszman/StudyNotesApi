using FluentAssertions;
using StudyNotesApi.Application.Common.Exceptions;
using StudyNotesApi.Application.Models.Common;
using StudyNotesApi.Application.Models.Notes;
using StudyNotesApi.Domain.Entities;
using StudyNotesApi.Infrastructure.Repositories;

namespace StudyNotesApi.UnitTests.Infrastructure.Repositories;

public class NoteRepositoryTests
{
    [Fact]
    public async Task AddAsync_and_RemoveAsync_should_persist_and_delete_a_note()
    {
        await using var dbContext = RepositoryTestContextFactory.Create();
        var repository = new NoteRepository(dbContext);
        var note = new Note("Study EF", "content", Guid.NewGuid());

        await repository.AddAsync(note);
        dbContext.Notes.Should().ContainSingle();

        await repository.RemoveAsync(note);
        dbContext.Notes.Should().BeEmpty();
    }

    [Fact]
    public async Task GetByIdAsync_should_only_return_notes_from_the_requested_user_and_include_note_tags()
    {
        await using var dbContext = RepositoryTestContextFactory.Create();
        var repository = new NoteRepository(dbContext);
        var userId = Guid.NewGuid();
        var note = new Note("Study EF", "content", userId);
        var tag = new Tag("dotnet", userId);
        var noteTag = new NoteTag(note.Id, tag.Id);

        await dbContext.Tags.AddAsync(tag);
        await repository.AddAsync(note);
        await dbContext.NoteTags.AddAsync(noteTag);
        await dbContext.SaveChangesAsync();

        var found = await repository.GetByIdAsync(note.Id, userId);
        var notFound = await repository.GetByIdAsync(note.Id, Guid.NewGuid());

        found.Should().NotBeNull();
        found!.NoteTags.Should().ContainSingle();
        notFound.Should().BeNull();
    }

    [Fact]
    public async Task GetByUserAsync_should_apply_all_filters_and_sorting()
    {
        await using var dbContext = RepositoryTestContextFactory.Create();
        var repository = new NoteRepository(dbContext);
        var userId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var tagId = Guid.NewGuid();

        var matchingNote = new Note("Study EF Core", "Relationships", userId, categoryId);
        matchingNote.SetFavorite(true);
        matchingNote.SetArchived(false);
        matchingNote.SetPinned(true);

        var excludedByFlags = new Note("Study LINQ", "Queries", userId, categoryId);
        excludedByFlags.SetFavorite(false);
        excludedByFlags.SetArchived(false);
        excludedByFlags.SetPinned(true);

        var excludedByUser = new Note("Study EF Core", "Relationships", Guid.NewGuid(), categoryId);
        excludedByUser.SetFavorite(true);
        excludedByUser.SetArchived(false);
        excludedByUser.SetPinned(true);

        await repository.AddAsync(matchingNote);
        await repository.AddAsync(excludedByFlags);
        await repository.AddAsync(excludedByUser);

        await dbContext.NoteTags.AddRangeAsync(
            new NoteTag(matchingNote.Id, tagId),
            new NoteTag(excludedByFlags.Id, Guid.NewGuid()));
        await dbContext.SaveChangesAsync();

        var result = await repository.GetByUserAsync(
            userId,
            new NoteFilter(
                search: "  EF Core  ",
                categoryId: categoryId,
                tagId: tagId,
                isFavorite: true,
                isArchived: false,
                isPinned: true),
            new PagedQuery(1, 10),
            new SortRequest("title", SortDirection.Asc));

        result.TotalCount.Should().Be(1);
        result.Items.Select(note => note.Title).Should().ContainSingle().Which.Should().Be("Study EF Core");
    }

    [Fact]
    public async Task GetByUserAsync_should_apply_default_sorting_and_pagination()
    {
        await using var dbContext = RepositoryTestContextFactory.Create();
        var repository = new NoteRepository(dbContext);
        var userId = Guid.NewGuid();

        var older = new Note("Older", "content", userId);
        var newer = new Note("Newer", "content", userId);

        EntityPropertySetter.SetProperty(older, nameof(Note.CreatedAt), new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc));
        EntityPropertySetter.SetProperty(newer, nameof(Note.CreatedAt), new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc));

        await repository.AddAsync(older);
        await repository.AddAsync(newer);

        var result = await repository.GetByUserAsync(
            userId,
            new NoteFilter(),
            new PagedQuery(1, 1),
            new SortRequest());

        result.Items.Select(note => note.Title).Should().ContainSingle().Which.Should().Be("Newer");
        result.HasNextPage.Should().BeTrue();
    }

    [Fact]
    public async Task GetByUserAsync_should_throw_for_invalid_sort_field()
    {
        await using var dbContext = RepositoryTestContextFactory.Create();
        var repository = new NoteRepository(dbContext);

        var action = async () => await repository.GetByUserAsync(
            Guid.NewGuid(),
            new NoteFilter(),
            new PagedQuery(),
            new SortRequest("invalidField"));

        await action.Should().ThrowAsync<ValidationException>();
    }
}
