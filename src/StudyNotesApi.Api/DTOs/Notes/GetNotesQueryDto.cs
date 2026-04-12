namespace StudyNotesApi.Api.DTOs.Notes;

public sealed class GetNotesQueryDto
{
    public string? Search { get; init; }

    public Guid? CategoryId { get; init; }

    public Guid? TagId { get; init; }

    public bool? IsFavorite { get; init; }

    public bool? IsArchived { get; init; }

    public bool? IsPinned { get; init; }

    public int PageNumber { get; init; } = 1;

    public int PageSize { get; init; } = 10;

    public string? SortBy { get; init; }

    public string? SortDirection { get; init; }
}
