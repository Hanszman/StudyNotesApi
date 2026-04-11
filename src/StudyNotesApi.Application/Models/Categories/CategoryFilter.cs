namespace StudyNotesApi.Application.Models.Categories;

public sealed class CategoryFilter
{
    public CategoryFilter(string? search = null)
    {
        Search = NormalizeSearch(search);
    }

    public string? Search { get; }

    private static string? NormalizeSearch(string? search)
    {
        return string.IsNullOrWhiteSpace(search) ? null : search.Trim();
    }
}
