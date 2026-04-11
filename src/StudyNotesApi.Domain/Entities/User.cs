using StudyNotesApi.Domain.Common;

namespace StudyNotesApi.Domain.Entities;

public class User : BaseEntity
{
    private readonly List<Category> _categories = [];
    private readonly List<Note> _notes = [];
    private readonly List<Tag> _tags = [];

    private User()
    {
    }

    public User(string name, string email, string passwordHash)
        : base(Guid.NewGuid())
    {
        Name = NormalizeRequired(name, nameof(name));
        Email = NormalizeEmail(email);
        PasswordHash = NormalizeRequired(passwordHash, nameof(passwordHash));
    }

    public string Name { get; private set; } = string.Empty;

    public string Email { get; private set; } = string.Empty;

    public string PasswordHash { get; private set; } = string.Empty;

    public IReadOnlyCollection<Category> Categories => _categories;

    public IReadOnlyCollection<Note> Notes => _notes;

    public IReadOnlyCollection<Tag> Tags => _tags;

    public void UpdateName(string name)
    {
        Name = NormalizeRequired(name, nameof(name));
        MarkAsUpdated();
    }

    public void UpdateEmail(string email)
    {
        Email = NormalizeEmail(email);
        MarkAsUpdated();
    }

    public void UpdatePasswordHash(string passwordHash)
    {
        PasswordHash = NormalizeRequired(passwordHash, nameof(passwordHash));
        MarkAsUpdated();
    }

    private static string NormalizeEmail(string email)
    {
        return NormalizeRequired(email, nameof(email)).ToLowerInvariant();
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
