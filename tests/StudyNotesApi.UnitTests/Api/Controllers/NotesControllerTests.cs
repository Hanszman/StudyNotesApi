using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using StudyNotesApi.Api.Controllers;
using StudyNotesApi.Api.DTOs.Common;
using StudyNotesApi.Api.DTOs.Notes;
using StudyNotesApi.Application.Common.Exceptions;
using StudyNotesApi.Application.Interfaces.Security;
using StudyNotesApi.Application.Interfaces.Services;
using StudyNotesApi.Application.Models.Common;
using StudyNotesApi.Application.Models.Notes;
using StudyNotesApi.Domain.Entities;

namespace StudyNotesApi.UnitTests.Api.Controllers;

public class NotesControllerTests
{
    [Fact]
    public async Task GetAll_should_return_paged_notes_for_the_current_user()
    {
        var noteService = new Mock<INoteService>();
        var currentUserService = new Mock<ICurrentUserService>();
        var controller = new NotesController(noteService.Object, currentUserService.Object);
        var userId = Guid.NewGuid();
        var note = new Note("Study EF", "content", userId);
        var tagId = Guid.NewGuid();
        note.ReplaceTags([tagId]);
        var pagedResult = new PagedResult<Note>([note], 2, 5, 9);

        currentUserService.Setup(value => value.GetRequiredUserId()).Returns(userId);
        noteService.Setup(value => value.GetNotesAsync(
                userId,
                It.Is<NoteFilter>(filter =>
                    filter.Search == "ef" &&
                    filter.CategoryId == null &&
                    filter.TagId == tagId &&
                    filter.IsFavorite == true &&
                    filter.IsArchived == false &&
                    filter.IsPinned == true),
                It.Is<PagedQuery>(query => query.PageNumber == 2 && query.PageSize == 5),
                It.Is<SortRequest>(sort => sort.SortBy == "title" && sort.Direction == SortDirection.Asc),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);

        var result = await controller.GetAll(new GetNotesQueryDto
        {
            Search = "ef",
            TagId = tagId,
            IsFavorite = true,
            IsArchived = false,
            IsPinned = true,
            PageNumber = 2,
            PageSize = 5,
            SortBy = "title",
            SortDirection = "asc"
        }, CancellationToken.None);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<PagedResponseDto<NoteResponseDto>>().Subject;

        response.Items.Should().ContainSingle();
        response.PageNumber.Should().Be(2);
        response.PageSize.Should().Be(5);
        response.TotalCount.Should().Be(9);
    }

    [Fact]
    public async Task GetAll_should_throw_validation_exception_for_invalid_sort_direction()
    {
        var controller = new NotesController(Mock.Of<INoteService>(), Mock.Of<ICurrentUserService>());

        var action = async () => await controller.GetAll(new GetNotesQueryDto
        {
            SortDirection = "sideways"
        }, CancellationToken.None);

        await action.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task GetAll_should_use_default_sort_direction_when_query_does_not_provide_one()
    {
        var noteService = new Mock<INoteService>();
        var currentUserService = new Mock<ICurrentUserService>();
        var controller = new NotesController(noteService.Object, currentUserService.Object);
        var userId = Guid.NewGuid();

        currentUserService.Setup(value => value.GetRequiredUserId()).Returns(userId);
        noteService.Setup(value => value.GetNotesAsync(
                userId,
                It.IsAny<NoteFilter>(),
                It.IsAny<PagedQuery>(),
                It.Is<SortRequest>(sort => sort.SortBy == "createdAt" && sort.Direction == SortDirection.Desc),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedResult<Note>([], 1, 10, 0));

        var result = await controller.GetAll(new GetNotesQueryDto
        {
            SortBy = "createdAt"
        }, CancellationToken.None);

        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetById_should_return_the_requested_note()
    {
        var noteService = new Mock<INoteService>();
        var currentUserService = new Mock<ICurrentUserService>();
        var controller = new NotesController(noteService.Object, currentUserService.Object);
        var userId = Guid.NewGuid();
        var note = new Note("Study EF", "content", userId, Guid.NewGuid());
        var tagId = Guid.NewGuid();
        note.ReplaceTags([tagId]);
        note.SetFavorite(true);
        note.SetPinned(true);

        currentUserService.Setup(value => value.GetRequiredUserId()).Returns(userId);
        noteService.Setup(value => value.GetByIdAsync(userId, note.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(note);

        var result = await controller.GetById(note.Id, CancellationToken.None);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(new NoteResponseDto(
            note.Id,
            note.Title,
            note.Content,
            note.CategoryId,
            [tagId],
            note.IsFavorite,
            note.IsArchived,
            note.IsPinned,
            note.CreatedAt,
            note.UpdatedAt));
    }

    [Fact]
    public async Task Create_should_return_created_at_action_with_note_payload()
    {
        var noteService = new Mock<INoteService>();
        var currentUserService = new Mock<ICurrentUserService>();
        var controller = new NotesController(noteService.Object, currentUserService.Object);
        var userId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var tagIds = new[] { Guid.NewGuid(), Guid.NewGuid() };
        var note = new Note("Study EF", "content", userId, categoryId);
        note.ReplaceTags(tagIds);

        currentUserService.Setup(value => value.GetRequiredUserId()).Returns(userId);
        noteService.Setup(value => value.CreateAsync(
                userId,
                "Study EF",
                "content",
                categoryId,
                It.Is<IReadOnlyCollection<Guid>>(value => value.SequenceEqual(tagIds)),
                true,
                false,
                true,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(note);

        var result = await controller.Create(new CreateNoteRequestDto(
            "Study EF",
            "content",
            categoryId,
            tagIds,
            true,
            false,
            true), CancellationToken.None);

        var createdResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.ActionName.Should().Be(nameof(NotesController.GetById));
        createdResult.RouteValues.Should().ContainKey("id").WhoseValue.Should().Be(note.Id);
    }

    [Fact]
    public async Task Update_should_return_ok_with_updated_note()
    {
        var noteService = new Mock<INoteService>();
        var currentUserService = new Mock<ICurrentUserService>();
        var controller = new NotesController(noteService.Object, currentUserService.Object);
        var userId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var tagId = Guid.NewGuid();
        var note = new Note("Study EF", "content", userId, categoryId);
        note.ReplaceTags([tagId]);

        currentUserService.Setup(value => value.GetRequiredUserId()).Returns(userId);
        noteService.Setup(value => value.UpdateAsync(
                userId,
                note.Id,
                "Study EF",
                "content",
                categoryId,
                It.Is<IReadOnlyCollection<Guid>>(value => value.SequenceEqual(new[] { tagId })),
                false,
                true,
                false,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(note);

        var result = await controller.Update(note.Id, new UpdateNoteRequestDto(
            "Study EF",
            "content",
            categoryId,
            [tagId],
            false,
            true,
            false), CancellationToken.None);

        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Delete_should_return_no_content_after_removing_note()
    {
        var noteService = new Mock<INoteService>();
        var currentUserService = new Mock<ICurrentUserService>();
        var controller = new NotesController(noteService.Object, currentUserService.Object);
        var userId = Guid.NewGuid();
        var noteId = Guid.NewGuid();

        currentUserService.Setup(value => value.GetRequiredUserId()).Returns(userId);

        var result = await controller.Delete(noteId, CancellationToken.None);

        result.Should().BeOfType<NoContentResult>();
        noteService.Verify(value => value.DeleteAsync(userId, noteId, It.IsAny<CancellationToken>()), Times.Once);
    }
}
