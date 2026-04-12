using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudyNotesApi.Api.DTOs.Common;
using StudyNotesApi.Api.DTOs.Notes;
using StudyNotesApi.Application.Common.Exceptions;
using StudyNotesApi.Application.Interfaces.Security;
using StudyNotesApi.Application.Interfaces.Services;
using StudyNotesApi.Application.Models.Common;
using StudyNotesApi.Application.Models.Notes;
using StudyNotesApi.Domain.Entities;

namespace StudyNotesApi.Api.Controllers;

[ApiController]
[Route("api/notes")]
[Authorize]
public class NotesController : ControllerBase
{
    private readonly ICurrentUserService _currentUserService;
    private readonly INoteService _noteService;

    public NotesController(INoteService noteService, ICurrentUserService currentUserService)
    {
        _noteService = noteService;
        _currentUserService = currentUserService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResponseDto<NoteResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PagedResponseDto<NoteResponseDto>>> GetAll(
        [FromQuery] GetNotesQueryDto query,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetRequiredUserId();
        var result = await _noteService.GetNotesAsync(
            userId,
            new NoteFilter(
                query.Search,
                query.CategoryId,
                query.TagId,
                query.IsFavorite,
                query.IsArchived,
                query.IsPinned),
            new PagedQuery(query.PageNumber, query.PageSize),
            BuildSortRequest(query.SortBy, query.SortDirection),
            cancellationToken);

        return Ok(ToPagedResponse(result));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(NoteResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<NoteResponseDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetRequiredUserId();
        var note = await _noteService.GetByIdAsync(userId, id, cancellationToken);

        return Ok(ToResponse(note));
    }

    [HttpPost]
    [ProducesResponseType(typeof(NoteResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<NoteResponseDto>> Create(
        [FromBody] CreateNoteRequestDto request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetRequiredUserId();
        var note = await _noteService.CreateAsync(
            userId,
            request.Title,
            request.Content,
            request.CategoryId,
            request.TagIds ?? Array.Empty<Guid>(),
            request.IsFavorite,
            request.IsArchived,
            request.IsPinned,
            cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = note.Id }, ToResponse(note));
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(NoteResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<NoteResponseDto>> Update(
        Guid id,
        [FromBody] UpdateNoteRequestDto request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetRequiredUserId();
        var note = await _noteService.UpdateAsync(
            userId,
            id,
            request.Title,
            request.Content,
            request.CategoryId,
            request.TagIds ?? Array.Empty<Guid>(),
            request.IsFavorite,
            request.IsArchived,
            request.IsPinned,
            cancellationToken);

        return Ok(ToResponse(note));
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetRequiredUserId();
        await _noteService.DeleteAsync(userId, id, cancellationToken);

        return NoContent();
    }

    private static SortRequest BuildSortRequest(string? sortBy, string? sortDirection)
    {
        if (string.IsNullOrWhiteSpace(sortDirection))
        {
            return new SortRequest(sortBy);
        }

        if (!Enum.TryParse<SortDirection>(sortDirection, true, out var direction))
        {
            throw new ValidationException("Sort direction must be either 'asc' or 'desc'.");
        }

        return new SortRequest(sortBy, direction);
    }

    private static PagedResponseDto<NoteResponseDto> ToPagedResponse(PagedResult<Note> result)
    {
        return new PagedResponseDto<NoteResponseDto>(
            result.Items.Select(ToResponse).ToArray(),
            result.PageNumber,
            result.PageSize,
            result.TotalCount,
            result.TotalPages,
            result.HasPreviousPage,
            result.HasNextPage);
    }

    private static NoteResponseDto ToResponse(Note note)
    {
        return new NoteResponseDto(
            note.Id,
            note.Title,
            note.Content,
            note.CategoryId,
            note.NoteTags.Select(noteTag => noteTag.TagId).ToArray(),
            note.IsFavorite,
            note.IsArchived,
            note.IsPinned,
            note.CreatedAt,
            note.UpdatedAt);
    }
}
