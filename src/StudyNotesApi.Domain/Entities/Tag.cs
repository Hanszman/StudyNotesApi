using StudyNotesApi.Domain.Common;

namespace StudyNotesApi.Domain.Entities;

public class Tag : BaseEntity
{
    private readonly List<NoteTag> _noteTags = [];

    private Tag()
    {
    }

    public Tag(string name, Guid userId)
        : base(Guid.NewGuid())
    {
        UserId = EnsureNotEmpty(userId, nameof(userId));
        Name = NormalizeRequired(name, nameof(name));
    }

    public string Name { get; private set; } = string.Empty;

    public Guid UserId { get; private set; }

    public User? User { get; private set; }

    public IReadOnlyCollection<NoteTag> NoteTags => _noteTags;

    public void Rename(string name)
    {
        Name = NormalizeRequired(name, nameof(name));
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

    private static string NormalizeRequired(string value, string paramName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException($"{paramName} cannot be empty.", paramName);
        }

        return value.Trim();
    }
}
