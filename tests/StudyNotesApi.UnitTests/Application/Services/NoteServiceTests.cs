using FluentAssertions;
using Moq;
using StudyNotesApi.Application.Common.Exceptions;
using StudyNotesApi.Application.Interfaces.Repositories;
using StudyNotesApi.Application.Models.Common;
using StudyNotesApi.Application.Models.Notes;
using StudyNotesApi.Application.Services;
using StudyNotesApi.Domain.Entities;

namespace StudyNotesApi.UnitTests.Application.Services;

public class NoteServiceTests
{
    [Fact]
    public async Task GetNotesAsync_should_delegate_to_repository()
    {
        var noteRepository = new Mock<INoteRepository>();
        var service = new NoteService(noteRepository.Object, Mock.Of<ICategoryRepository>(), Mock.Of<ITagRepository>());
        var userId = Guid.NewGuid();
        var expected = new PagedResult<Note>([], 1, 10, 0);

        noteRepository.Setup(value => value.GetByUserAsync(
                userId,
                It.IsAny<NoteFilter>(),
                It.IsAny<PagedQuery>(),
                It.IsAny<SortRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await service.GetNotesAsync(
            userId,
            new NoteFilter(search: "ef core", isPinned: true),
            new PagedQuery(),
            new SortRequest("title", SortDirection.Asc));

        result.Should().BeSameAs(expected);
    }

    [Fact]
    public async Task GetByIdAsync_should_return_note_when_it_exists()
    {
        var noteRepository = new Mock<INoteRepository>();
        var service = new NoteService(noteRepository.Object, Mock.Of<ICategoryRepository>(), Mock.Of<ITagRepository>());
        var note = new Note("Study EF", "content", Guid.NewGuid());

        noteRepository.Setup(value => value.GetByIdAsync(note.Id, note.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(note);

        var result = await service.GetByIdAsync(note.UserId, note.Id);

        result.Should().BeSameAs(note);
    }

    [Fact]
    public async Task GetByIdAsync_should_throw_not_found_when_note_does_not_exist()
    {
        var noteRepository = new Mock<INoteRepository>();
        var service = new NoteService(noteRepository.Object, Mock.Of<ICategoryRepository>(), Mock.Of<ITagRepository>());

        noteRepository.Setup(value => value.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Note?)null);

        var action = async () => await service.GetByIdAsync(Guid.NewGuid(), Guid.NewGuid());

        await action.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task CreateAsync_should_create_note_when_category_and_tags_belong_to_user()
    {
        var noteRepository = new Mock<INoteRepository>();
        var categoryRepository = new Mock<ICategoryRepository>();
        var tagRepository = new Mock<ITagRepository>();
        var service = new NoteService(noteRepository.Object, categoryRepository.Object, tagRepository.Object);
        var userId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var tagA = new Tag("dotnet", userId);
        var tagB = new Tag("efcore", userId);

        categoryRepository.Setup(value => value.GetByIdAsync(categoryId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Category("Backend", userId));
        tagRepository.Setup(value => value.GetByIdsAsync(userId, It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([tagA, tagB]);

        var result = await service.CreateAsync(
            userId,
            "  Study EF Core  ",
            "  Relationships  ",
            categoryId,
            [tagA.Id, tagB.Id, tagA.Id],
            true,
            false,
            true);

        result.Title.Should().Be("Study EF Core");
        result.Content.Should().Be("Relationships");
        result.CategoryId.Should().Be(categoryId);
        result.IsFavorite.Should().BeTrue();
        result.IsArchived.Should().BeFalse();
        result.IsPinned.Should().BeTrue();
        result.NoteTags.Select(noteTag => noteTag.TagId).Should().BeEquivalentTo([tagA.Id, tagB.Id]);
        noteRepository.Verify(value => value.AddAsync(It.IsAny<Note>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_should_throw_validation_exception_when_title_is_missing()
    {
        var service = new NoteService(Mock.Of<INoteRepository>(), Mock.Of<ICategoryRepository>(), Mock.Of<ITagRepository>());

        var action = async () => await service.CreateAsync(
            Guid.NewGuid(),
            " ",
            "content",
            null,
            [],
            false,
            false,
            false);

        await action.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task CreateAsync_should_throw_validation_exception_when_category_does_not_belong_to_user()
    {
        var categoryRepository = new Mock<ICategoryRepository>();
        var service = new NoteService(Mock.Of<INoteRepository>(), categoryRepository.Object, Mock.Of<ITagRepository>());

        categoryRepository.Setup(value => value.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Category?)null);

        var action = async () => await service.CreateAsync(
            Guid.NewGuid(),
            "Study EF",
            "content",
            Guid.NewGuid(),
            [],
            false,
            false,
            false);

        await action.Should().ThrowAsync<ValidationException>()
            .WithMessage("Category must belong to the current user.");
    }

    [Fact]
    public async Task CreateAsync_should_throw_validation_exception_when_any_tag_does_not_belong_to_user()
    {
        var tagRepository = new Mock<ITagRepository>();
        var service = new NoteService(Mock.Of<INoteRepository>(), Mock.Of<ICategoryRepository>(), tagRepository.Object);
        var userId = Guid.NewGuid();
        var tag = new Tag("dotnet", userId);

        tagRepository.Setup(value => value.GetByIdsAsync(userId, It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([tag]);

        var action = async () => await service.CreateAsync(
            userId,
            "Study EF",
            "content",
            null,
            [tag.Id, Guid.NewGuid()],
            false,
            false,
            false);

        await action.Should().ThrowAsync<ValidationException>()
            .WithMessage("All tags must belong to the current user.");
    }

    [Fact]
    public async Task CreateAsync_should_allow_empty_tag_collection_without_querying_tags()
    {
        var tagRepository = new Mock<ITagRepository>();
        var service = new NoteService(Mock.Of<INoteRepository>(), Mock.Of<ICategoryRepository>(), tagRepository.Object);

        await service.CreateAsync(
            Guid.NewGuid(),
            "Study EF",
            "content",
            null,
            [],
            false,
            false,
            false);

        tagRepository.Verify(value => value.GetByIdsAsync(It.IsAny<Guid>(), It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_should_throw_validation_exception_when_tag_ids_collection_is_null()
    {
        var service = new NoteService(Mock.Of<INoteRepository>(), Mock.Of<ICategoryRepository>(), Mock.Of<ITagRepository>());

        var action = async () => await service.CreateAsync(
            Guid.NewGuid(),
            "Study EF",
            "content",
            null,
            null!,
            false,
            false,
            false);

        await action.Should().ThrowAsync<ValidationException>()
            .WithMessage("Tag IDs are required.");
    }

    [Fact]
    public async Task CreateAsync_should_throw_validation_exception_when_tag_ids_contain_empty_guid()
    {
        var service = new NoteService(Mock.Of<INoteRepository>(), Mock.Of<ICategoryRepository>(), Mock.Of<ITagRepository>());

        var action = async () => await service.CreateAsync(
            Guid.NewGuid(),
            "Study EF",
            "content",
            null,
            [Guid.Empty],
            false,
            false,
            false);

        await action.Should().ThrowAsync<ValidationException>()
            .WithMessage("Tag IDs cannot contain empty values.");
    }

    [Fact]
    public async Task UpdateAsync_should_update_note_and_sync_tags()
    {
        var noteRepository = new Mock<INoteRepository>();
        var categoryRepository = new Mock<ICategoryRepository>();
        var tagRepository = new Mock<ITagRepository>();
        var service = new NoteService(noteRepository.Object, categoryRepository.Object, tagRepository.Object);
        var userId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var note = new Note("Old title", "old content", userId);
        var tagA = new Tag("dotnet", userId);
        var tagB = new Tag("efcore", userId);

        noteRepository.Setup(value => value.GetByIdAsync(note.Id, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(note);
        categoryRepository.Setup(value => value.GetByIdAsync(categoryId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Category("Backend", userId));
        tagRepository.Setup(value => value.GetByIdsAsync(userId, It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([tagA, tagB]);

        var result = await service.UpdateAsync(
            userId,
            note.Id,
            "  New title  ",
            "  new content  ",
            categoryId,
            [tagA.Id, tagB.Id],
            true,
            true,
            false);

        result.Title.Should().Be("New title");
        result.Content.Should().Be("new content");
        result.CategoryId.Should().Be(categoryId);
        result.IsFavorite.Should().BeTrue();
        result.IsArchived.Should().BeTrue();
        result.IsPinned.Should().BeFalse();
        result.NoteTags.Select(noteTag => noteTag.TagId).Should().BeEquivalentTo([tagA.Id, tagB.Id]);
        noteRepository.Verify(value => value.UpdateAsync(note, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_should_throw_not_found_when_note_does_not_exist()
    {
        var noteRepository = new Mock<INoteRepository>();
        var service = new NoteService(noteRepository.Object, Mock.Of<ICategoryRepository>(), Mock.Of<ITagRepository>());

        noteRepository.Setup(value => value.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Note?)null);

        var action = async () => await service.UpdateAsync(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Title",
            "content",
            null,
            [],
            false,
            false,
            false);

        await action.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task DeleteAsync_should_remove_note_when_it_exists()
    {
        var noteRepository = new Mock<INoteRepository>();
        var service = new NoteService(noteRepository.Object, Mock.Of<ICategoryRepository>(), Mock.Of<ITagRepository>());
        var note = new Note("Study EF", "content", Guid.NewGuid());

        noteRepository.Setup(value => value.GetByIdAsync(note.Id, note.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(note);

        await service.DeleteAsync(note.UserId, note.Id);

        noteRepository.Verify(value => value.RemoveAsync(note, It.IsAny<CancellationToken>()), Times.Once);
    }
}
