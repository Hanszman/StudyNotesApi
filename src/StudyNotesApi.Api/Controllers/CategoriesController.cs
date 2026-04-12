using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudyNotesApi.Api.DTOs.Categories;
using StudyNotesApi.Api.DTOs.Common;
using StudyNotesApi.Application.Common.Exceptions;
using StudyNotesApi.Application.Interfaces.Security;
using StudyNotesApi.Application.Interfaces.Services;
using StudyNotesApi.Application.Models.Categories;
using StudyNotesApi.Application.Models.Common;
using StudyNotesApi.Domain.Entities;

namespace StudyNotesApi.Api.Controllers;

[ApiController]
[Route("api/categories")]
[Authorize]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _categoryService;
    private readonly ICurrentUserService _currentUserService;

    public CategoriesController(ICategoryService categoryService, ICurrentUserService currentUserService)
    {
        _categoryService = categoryService;
        _currentUserService = currentUserService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResponseDto<CategoryResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PagedResponseDto<CategoryResponseDto>>> GetAll(
        [FromQuery] GetCategoriesQueryDto query,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetRequiredUserId();
        var result = await _categoryService.GetCategoriesAsync(
            userId,
            new CategoryFilter(query.Search),
            new PagedQuery(query.PageNumber, query.PageSize),
            BuildSortRequest(query.SortBy, query.SortDirection),
            cancellationToken);

        return Ok(ToPagedResponse(result));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(CategoryResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CategoryResponseDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetRequiredUserId();
        var category = await _categoryService.GetByIdAsync(userId, id, cancellationToken);

        return Ok(ToResponse(category));
    }

    [HttpPost]
    [ProducesResponseType(typeof(CategoryResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CategoryResponseDto>> Create(
        [FromBody] CreateCategoryRequestDto request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetRequiredUserId();
        var category = await _categoryService.CreateAsync(userId, request.Name, request.Color, cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = category.Id }, ToResponse(category));
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(CategoryResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CategoryResponseDto>> Update(
        Guid id,
        [FromBody] UpdateCategoryRequestDto request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetRequiredUserId();
        var category = await _categoryService.UpdateAsync(userId, id, request.Name, request.Color, cancellationToken);

        return Ok(ToResponse(category));
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetRequiredUserId();
        await _categoryService.DeleteAsync(userId, id, cancellationToken);

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

    private static PagedResponseDto<CategoryResponseDto> ToPagedResponse(PagedResult<Category> result)
    {
        return new PagedResponseDto<CategoryResponseDto>(
            result.Items.Select(ToResponse).ToArray(),
            result.PageNumber,
            result.PageSize,
            result.TotalCount,
            result.TotalPages,
            result.HasPreviousPage,
            result.HasNextPage);
    }

    private static CategoryResponseDto ToResponse(Category category)
    {
        return new CategoryResponseDto(
            category.Id,
            category.Name,
            category.Color,
            category.CreatedAt,
            category.UpdatedAt);
    }
}
