using StudyNotesApi.Domain.Common;

namespace StudyNotesApi.Domain.Entities;

public class Note : BaseEntity
{
    private readonly List<NoteTag> _noteTags = [];

    private Note()
    {
    }

    public Note(string title, string content, Guid userId, Guid? categoryId = null)
        : base(Guid.NewGuid())
    {
        UserId = EnsureNotEmpty(userId, nameof(userId));
        Title = NormalizeRequired(title, nameof(title));
        Content = NormalizeContent(content);
        CategoryId = NormalizeOptionalGuid(categoryId);
    }

    public string Title { get; private set; } = string.Empty;

    public string Content { get; private set; } = string.Empty;

    public bool IsFavorite { get; private set; }

    public bool IsArchived { get; private set; }

    public bool IsPinned { get; private set; }

    public Guid UserId { get; private set; }

    public Guid? CategoryId { get; private set; }

    public User? User { get; private set; }

    public Category? Category { get; private set; }

    public IReadOnlyCollection<NoteTag> NoteTags => _noteTags;

    public void UpdateTitle(string title)
    {
        Title = NormalizeRequired(title, nameof(title));
        MarkAsUpdated();
    }

    public void UpdateContent(string content)
    {
        Content = NormalizeContent(content);
        MarkAsUpdated();
    }

    public void SetCategory(Guid? categoryId)
    {
        CategoryId = NormalizeOptionalGuid(categoryId);
        MarkAsUpdated();
    }

    public void SetFavorite(bool isFavorite)
    {
        IsFavorite = isFavorite;
        MarkAsUpdated();
    }

    public void SetArchived(bool isArchived)
    {
        IsArchived = isArchived;
        MarkAsUpdated();
    }

    public void SetPinned(bool isPinned)
    {
        IsPinned = isPinned;
        MarkAsUpdated();
    }

    public void ReplaceTags(IReadOnlyCollection<Guid> tagIds)
    {
        _noteTags.Clear();

        foreach (var tagId in NormalizeTagIds(tagIds))
        {
            _noteTags.Add(new NoteTag(Id, tagId));
        }

        MarkAsUpdated();
    }

    private static Guid EnsureNotEmpty(Guid value, string paramName)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException($"{paramName} cannot be empty.", paramName);
        }

        return value;
    }

    private static Guid? NormalizeOptionalGuid(Guid? value)
    {
        return value == Guid.Empty ? null : value;
    }

    private static string NormalizeRequired(string value, string paramName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException($"{paramName} cannot be empty.", paramName);
        }

        return value.Trim();
    }

    private static string NormalizeContent(string content)
    {
        return string.IsNullOrWhiteSpace(content) ? string.Empty : content.Trim();
    }

    private static IReadOnlyCollection<Guid> NormalizeTagIds(IReadOnlyCollection<Guid> tagIds)
    {
        if (tagIds is null)
        {
            throw new ArgumentNullException(nameof(tagIds));
        }

        var normalizedTagIds = new List<Guid>();
        foreach (var tagId in tagIds)
        {
            if (tagId == Guid.Empty)
            {
                throw new ArgumentException("tagIds cannot contain empty values.", nameof(tagIds));
            }

            if (!normalizedTagIds.Contains(tagId))
            {
                normalizedTagIds.Add(tagId);
            }
        }

        return normalizedTagIds;
    }
}
