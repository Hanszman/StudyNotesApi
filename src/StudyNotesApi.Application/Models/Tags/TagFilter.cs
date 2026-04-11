namespace StudyNotesApi.Application.Models.Tags;

public sealed class TagFilter
{
    public TagFilter(string? search = null)
    {
        Search = NormalizeSearch(search);
    }

    public string? Search { get; }

    private static string? NormalizeSearch(string? search)
    {
        return string.IsNullOrWhiteSpace(search) ? null : search.Trim();
    }
}
