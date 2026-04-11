using StudyNotesApi.Application.Common.Exceptions;

namespace StudyNotesApi.Application.Models.Common;

public sealed class PagedResult<T>
{
    public PagedResult(IReadOnlyCollection<T> items, int pageNumber, int pageSize, int totalCount)
    {
        if (pageNumber < 1)
        {
            throw new ValidationException("Page number must be greater than or equal to 1.");
        }

        if (pageSize < 1)
        {
            throw new ValidationException("Page size must be greater than or equal to 1.");
        }

        if (totalCount < 0)
        {
            throw new ValidationException("Total count cannot be negative.");
        }

        Items = items ?? throw new ArgumentNullException(nameof(items));
        PageNumber = pageNumber;
        PageSize = pageSize;
        TotalCount = totalCount;
    }

    public IReadOnlyCollection<T> Items { get; }

    public int PageNumber { get; }

    public int PageSize { get; }

    public int TotalCount { get; }

    public int TotalPages => TotalCount == 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);

    public bool HasPreviousPage => PageNumber > 1;

    public bool HasNextPage => PageNumber < TotalPages;
}
