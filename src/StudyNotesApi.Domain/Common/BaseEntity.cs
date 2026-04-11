namespace StudyNotesApi.Domain.Common;

public abstract class BaseEntity
{
    protected BaseEntity()
    {
    }

    protected BaseEntity(Guid? id)
    {
        Id = id.GetValueOrDefault(Guid.NewGuid());
        CreatedAt = DateTime.UtcNow;
    }

    public Guid Id { get; protected set; }

    public DateTime CreatedAt { get; protected set; }

    public DateTime? UpdatedAt { get; protected set; }

    protected void MarkAsUpdated()
    {
        UpdatedAt = DateTime.UtcNow;
    }
}
