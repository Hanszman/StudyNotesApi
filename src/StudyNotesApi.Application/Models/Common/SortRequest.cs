namespace StudyNotesApi.Application.Models.Common;

public sealed class SortRequest
{
    public SortRequest(string? sortBy = null, SortDirection direction = SortDirection.Desc)
    {
        SortBy = NormalizeSortBy(sortBy);
        Direction = direction;
    }

    public string? SortBy { get; }

    public SortDirection Direction { get; }

    public bool HasSorting => SortBy is not null;

    private static string? NormalizeSortBy(string? sortBy)
    {
        return string.IsNullOrWhiteSpace(sortBy) ? null : sortBy.Trim();
    }
}
