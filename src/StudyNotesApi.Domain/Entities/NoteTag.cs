namespace StudyNotesApi.Domain.Entities;

public class NoteTag
{
    private NoteTag()
    {
    }

    public NoteTag(Guid noteId, Guid tagId)
    {
        NoteId = EnsureNotEmpty(noteId, nameof(noteId));
        TagId = EnsureNotEmpty(tagId, nameof(tagId));
    }

    public Guid NoteId { get; private set; }

    public Note? Note { get; private set; }

    public Guid TagId { get; private set; }

    public Tag? Tag { get; private set; }

    private static Guid EnsureNotEmpty(Guid value, string paramName)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException($"{paramName} cannot be empty.", paramName);
        }

        return value;
    }
}
