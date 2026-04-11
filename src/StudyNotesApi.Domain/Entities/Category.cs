using StudyNotesApi.Domain.Common;

namespace StudyNotesApi.Domain.Entities;

public class Category : BaseEntity
{
    private readonly List<Note> _notes = [];

    private Category()
    {
    }

    public Category(string name, Guid userId, string? color = null)
        : base(Guid.NewGuid())
    {
        UserId = EnsureNotEmpty(userId, nameof(userId));
        Name = NormalizeRequired(name, nameof(name));
        Color = NormalizeOptional(color);
    }

    public string Name { get; private set; } = string.Empty;

    public string? Color { get; private set; }

    public Guid UserId { get; private set; }

    public User? User { get; private set; }

    public IReadOnlyCollection<Note> Notes => _notes;

    public void Rename(string name)
    {
        Name = NormalizeRequired(name, nameof(name));
        MarkAsUpdated();
    }

    public void UpdateColor(string? color)
    {
        Color = NormalizeOptional(color);
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

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
