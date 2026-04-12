namespace StudyNotesApi.Api.DTOs.Notes;

public sealed record NoteResponseDto(
    Guid Id,
    string Title,
    string Content,
    Guid? CategoryId,
    IReadOnlyCollection<Guid> TagIds,
    bool IsFavorite,
    bool IsArchived,
    bool IsPinned,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
