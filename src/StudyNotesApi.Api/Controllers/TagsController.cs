using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudyNotesApi.Api.DTOs.Common;
using StudyNotesApi.Api.DTOs.Tags;
using StudyNotesApi.Application.Common.Exceptions;
using StudyNotesApi.Application.Interfaces.Security;
using StudyNotesApi.Application.Interfaces.Services;
using StudyNotesApi.Application.Models.Common;
using StudyNotesApi.Application.Models.Tags;
using StudyNotesApi.Domain.Entities;

namespace StudyNotesApi.Api.Controllers;

[ApiController]
[Route("api/tags")]
[Authorize]
public class TagsController : ControllerBase
{
    private readonly ICurrentUserService _currentUserService;
    private readonly ITagService _tagService;

    public TagsController(ITagService tagService, ICurrentUserService currentUserService)
    {
        _tagService = tagService;
        _currentUserService = currentUserService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResponseDto<TagResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PagedResponseDto<TagResponseDto>>> GetAll(
        [FromQuery] GetTagsQueryDto query,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetRequiredUserId();
        var result = await _tagService.GetTagsAsync(
            userId,
            new TagFilter(query.Search),
            new PagedQuery(query.PageNumber, query.PageSize),
            BuildSortRequest(query.SortBy, query.SortDirection),
            cancellationToken);

        return Ok(ToPagedResponse(result));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(TagResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TagResponseDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetRequiredUserId();
        var tag = await _tagService.GetByIdAsync(userId, id, cancellationToken);

        return Ok(ToResponse(tag));
    }

    [HttpPost]
    [ProducesResponseType(typeof(TagResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<TagResponseDto>> Create(
        [FromBody] CreateTagRequestDto request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetRequiredUserId();
        var tag = await _tagService.CreateAsync(userId, request.Name, cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = tag.Id }, ToResponse(tag));
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(TagResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<TagResponseDto>> Update(
        Guid id,
        [FromBody] UpdateTagRequestDto request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetRequiredUserId();
        var tag = await _tagService.UpdateAsync(userId, id, request.Name, cancellationToken);

        return Ok(ToResponse(tag));
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetRequiredUserId();
        await _tagService.DeleteAsync(userId, id, cancellationToken);

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

    private static PagedResponseDto<TagResponseDto> ToPagedResponse(PagedResult<Tag> result)
    {
        return new PagedResponseDto<TagResponseDto>(
            result.Items.Select(ToResponse).ToArray(),
            result.PageNumber,
            result.PageSize,
            result.TotalCount,
            result.TotalPages,
            result.HasPreviousPage,
            result.HasNextPage);
    }

    private static TagResponseDto ToResponse(Tag tag)
    {
        return new TagResponseDto(
            tag.Id,
            tag.Name,
            tag.CreatedAt,
            tag.UpdatedAt);
    }
}
