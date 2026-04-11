using StudyNotesApi.Application.Common.Exceptions;

namespace StudyNotesApi.Application.Models.Common;

public sealed class PagedQuery
{
    public const int DefaultPageNumber = 1;
    public const int DefaultPageSize = 10;
    public const int MaxPageSize = 100;

    public PagedQuery(int pageNumber = DefaultPageNumber, int pageSize = DefaultPageSize)
    {
        if (pageNumber < 1)
        {
            throw new ValidationException("Page number must be greater than or equal to 1.");
        }

        if (pageSize < 1 || pageSize > MaxPageSize)
        {
            throw new ValidationException($"Page size must be between 1 and {MaxPageSize}.");
        }

        PageNumber = pageNumber;
        PageSize = pageSize;
    }

    public int PageNumber { get; }

    public int PageSize { get; }

    public int Skip => (PageNumber - 1) * PageSize;
}
