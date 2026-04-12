namespace StudyNotesApi.Api.DTOs.Notes;

public sealed record CreateNoteRequestDto(
    string Title,
    string Content,
    Guid? CategoryId,
    IReadOnlyCollection<Guid> TagIds,
    bool IsFavorite,
    bool IsArchived,
    bool IsPinned);
