namespace StudyNotesApi.Application.Models.Notes;

public sealed class NoteFilter
{
    public NoteFilter(
        string? search = null,
        Guid? categoryId = null,
        Guid? tagId = null,
        bool? isFavorite = null,
        bool? isArchived = null,
        bool? isPinned = null)
    {
        Search = NormalizeSearch(search);
        CategoryId = NormalizeOptionalGuid(categoryId);
        TagId = NormalizeOptionalGuid(tagId);
        IsFavorite = isFavorite;
        IsArchived = isArchived;
        IsPinned = isPinned;
    }

    public string? Search { get; }

    public Guid? CategoryId { get; }

    public Guid? TagId { get; }

    public bool? IsFavorite { get; }

    public bool? IsArchived { get; }

    public bool? IsPinned { get; }

    private static string? NormalizeSearch(string? search)
    {
        return string.IsNullOrWhiteSpace(search) ? null : search.Trim();
    }

    private static Guid? NormalizeOptionalGuid(Guid? value)
    {
        return value == Guid.Empty ? null : value;
    }
}
