namespace StudyNotesApi.Api.DTOs.Tags;

public sealed class GetTagsQueryDto
{
    public string? Search { get; init; }

    public int PageNumber { get; init; } = 1;

    public int PageSize { get; init; } = 10;

    public string? SortBy { get; init; }

    public string? SortDirection { get; init; }
}
