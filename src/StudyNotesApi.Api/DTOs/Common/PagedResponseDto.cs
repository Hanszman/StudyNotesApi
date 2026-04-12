namespace StudyNotesApi.Api.DTOs.Common;

public sealed record PagedResponseDto<T>(
    IReadOnlyCollection<T> Items,
    int PageNumber,
    int PageSize,
    int TotalCount,
    int TotalPages,
    bool HasPreviousPage,
    bool HasNextPage);
