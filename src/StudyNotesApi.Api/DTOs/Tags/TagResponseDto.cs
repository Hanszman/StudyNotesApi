namespace StudyNotesApi.Api.DTOs.Tags;

public sealed record TagResponseDto(
    Guid Id,
    string Name,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
