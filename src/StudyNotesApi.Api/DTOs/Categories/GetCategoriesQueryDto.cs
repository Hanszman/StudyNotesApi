namespace StudyNotesApi.Api.DTOs.Categories;

public sealed class GetCategoriesQueryDto
{
    public string? Search { get; init; }

    public int PageNumber { get; init; } = 1;

    public int PageSize { get; init; } = 10;

    public string? SortBy { get; init; }

    public string? SortDirection { get; init; }
}
